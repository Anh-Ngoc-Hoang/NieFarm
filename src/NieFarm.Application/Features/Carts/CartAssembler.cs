using Ardalis.Specification;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Coupons;
using NieFarm.Application.Features.Coupons.Specifications;
using NieFarm.Application.Features.Products;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Application.Features.Shipping;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;
using DomainCart = NieFarm.Domain.Entities.Cart;

namespace NieFarm.Application.Features.Carts;

/// <summary>
/// The single path used to build a CartDto for both display (GetCartQuery) and charge
/// (CreateOrderCommandHandler), plus every command that returns a rebuilt cart. Lines are
/// ordered by CartItem.Id (insertion order) — a stable, server-defined display order — so "what
/// you saw is what you are charged" is structural, not conventional. The price book is the
/// Products table, re-read on every call: name, image, variant label, price and availability are
/// never trusted from the stored CartItem row (security.md). A line whose product was deleted or
/// hidden is silently dropped (D8) — there is no data left to render it with, which is also what
/// keeps a hand-inserted CartItems row from ever being priced. A line whose variant is now
/// unavailable, or whose label combination no longer resolves to any variant, is kept and
/// flagged IsAvailable = false (D9) and excluded from Subtotal/ItemCount/ShippingFee, so the
/// displayed total is never a number the shopper cannot actually pay.
///
/// The coupon code the cart remembers is re-loaded and re-evaluated on every build (R-10): the
/// assembler decides eligibility (dates, limits, minimum, per-customer) from fresh reads and feeds
/// only an eligible coupon into CartPricing. It never redeems; redemption is the order row, written
/// by CreateOrderCommandHandler.
///
/// Public (not internal, despite the rest of this feature's helpers) because it is injected as
/// a constructor parameter into public MediatR handlers (GetCartQueryHandler,
/// AddToCartCommandHandler, SetCartLineQuantityCommandHandler, RemoveCartLineCommandHandler,
/// CreateOrderCommandHandler) — an internal type there would fail to compile (CS0053).
/// </summary>
public sealed class CartAssembler(
    IReadRepositoryBase<Product> products,
    IShippingRateProvider shippingRates,
    IReadRepositoryBase<Coupon> coupons,
    IReadRepositoryBase<Order> orders)
{
    public async Task<CartDto> BuildAsync(
        DomainCart? cart, CartPricingContext context, CancellationToken cancellationToken)
    {
        if (cart is null || cart.Items.Count == 0)
            return CartDto.Empty;

        var ids = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var rows = await products.ListAsync(new ProductsByIdsForCartSpec(ids), cancellationToken);
        var productsById = rows.ToDictionary(p => p.Id);

        var lines = new List<CartLineDto>();
        foreach (var item in cart.Items.OrderBy(i => i.Id))
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
                continue; // deleted or hidden — no data left to render (D8)

            var labels = CartLineKey.Split(item.VariantKey);
            var variant = ProductVariantResolver.FindVariant(product, labels);

            var unitPrice = variant?.Price ?? product.Price;
            var isAvailable = ProductAvailability.IsVariantPurchasable(product, variant);

            var imageAlt = string.IsNullOrWhiteSpace(product.ImageAlt) ? product.Name : product.ImageAlt;
            var quantity = Math.Clamp(item.Quantity, 1, DomainCart.MaxQuantityPerItem);

            lines.Add(new CartLineDto(
                product.Id,
                item.VariantKey,
                product.Slug,
                product.Name,
                CartLineKey.ToDisplay(labels),
                product.ImageUrl,
                imageAlt,
                unitPrice,
                quantity,
                unitPrice * quantity,
                isAvailable));
        }

        var availableLines = lines.Where(l => l.IsAvailable).ToList();
        var subtotal = availableLines.Sum(l => l.LineTotal);
        var itemCount = availableLines.Sum(l => l.Quantity);

        // The only place money is combined: shipping comes from the admin-managed rates for the
        // province the shopper chose (never from the browser as an amount).
        var rates = await shippingRates.GetRatesAsync(cancellationToken);
        var quote = ShippingZoneResolver.Resolve(rates, context.Province);
        var hasPayableLines = availableLines.Count > 0;

        // The coupon the cart remembers is re-loaded and re-evaluated on every build (D13), so a code that
        // expired, was edited or ran out is never trusted from the cart row.
        var evaluation = hasPayableLines
            ? await EvaluateCouponAsync(cart, context, subtotal, cancellationToken)
            : null;

        var totals = CartPricing.Calculate(
            subtotal, hasPayableLines, quote, rates.FreeShippingThreshold,
            evaluation?.Eligible is { } eligible ? CouponQuote.From(eligible) : null);

        var couponDto = evaluation is null ? null : evaluation.ToDto(totals.Discount);

        return new CartDto(lines, itemCount, totals.Subtotal, totals.ShippingFee, totals.Discount, totals.Total,
            quote.ZoneName, quote.ProvinceResolved, rates.FreeShippingThreshold, totals.FreeShippingRemaining,
            couponDto);
    }

    private sealed record CouponEvaluation(
        string Code, Coupon? Coupon, CouponEligibility Eligibility)
    {
        /// <summary>The coupon, only when it may be used on this cart.</summary>
        public Coupon? Eligible => Eligibility == CouponEligibility.Eligible ? Coupon : null;

        public CartCouponDto ToDto(decimal discount)
        {
            if (Coupon is null)
                return new CartCouponDto(Code, null, false, CouponMessages.Invalid, CouponEligibility.Inactive);

            if (Eligibility != CouponEligibility.Eligible)
                return new CartCouponDto(Code, Coupon.Type, false, CouponMessages.Reason(Eligibility, Coupon), Eligibility);

            // Eligible but lowering nothing (a FreeShipping code on an order already free): shown, not redeemed.
            if (discount <= 0m)
            {
                var message = Coupon.WaivesShipping ? CouponMessages.FreeShippingAlready : CouponMessages.NoDiscount;
                return new CartCouponDto(Code, Coupon.Type, false, message, Eligibility);
            }

            return new CartCouponDto(Code, Coupon.Type, true, null, Eligibility);
        }
    }

    private async Task<CouponEvaluation?> EvaluateCouponAsync(
        DomainCart cart, CartPricingContext context, decimal subtotal, CancellationToken cancellationToken)
    {
        var code = context.CouponCodeOverride ?? cart.CouponCode;
        if (string.IsNullOrWhiteSpace(code))
            return null;

        // The order handler hands in the row it loaded through the write repository (tracked, with its row
        // version) and that load happened BEFORE the counts below: either a concurrent order is visible in
        // the count, or the redemption UPDATE fails on the stale version (plan D15).
        var coupon = context.TrackedCoupon is { } tracked
                     && string.Equals(tracked.Code, code, StringComparison.OrdinalIgnoreCase)
            ? tracked
            : await coupons.FirstOrDefaultAsync(new CouponByCodeSpec(code), cancellationToken);

        if (coupon is null)
            return new CouponEvaluation(code, null, CouponEligibility.Inactive);

        var activeUses = await orders.CountAsync(new ActiveCouponOrdersSpec(coupon.Id), cancellationToken);

        // The customer is known by account (display paths) and additionally by phone (the order handler).
        int? customerUses = null;
        if (coupon.PerCustomerLimit is not null
            && (!string.IsNullOrWhiteSpace(context.UserId) || !string.IsNullOrWhiteSpace(context.PhoneKey)))
        {
            customerUses = await orders.CountAsync(
                new ActiveCouponOrdersForCustomerSpec(coupon.Id, context.UserId, context.PhoneKey), cancellationToken);
        }

        var eligibility = coupon.CheckEligibility(DateTime.UtcNow, subtotal, activeUses, customerUses);
        return new CouponEvaluation(coupon.Code, coupon, eligibility);
    }
}
