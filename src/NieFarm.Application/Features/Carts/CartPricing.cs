using NieFarm.Application.Features.Shipping;

namespace NieFarm.Application.Features.Carts;

/// <summary>The result of <see cref="CartPricing.Calculate"/>: every money value the cart shows or an order charges.</summary>
public record CartTotals(
    decimal Subtotal,
    decimal ShippingFee,
    decimal Discount,
    decimal Total,
    decimal? FreeShippingRemaining);

/// <summary>
/// The single pure calculator where money is combined (docs/plans/R-09-10-11-cart-coupons-shipping.md D4).
/// Only CartAssembler calls it, and both display and charge go through CartAssembler, so the two can
/// never disagree. Order of operations:
/// 1. Subtotal = sum of the available lines (passed in).
/// 2. Merchandise discount (coupons, Phase 3: not wired yet, always 0).
/// 3. Shipping fee: 0 with no payable lines; 0 when a threshold is set and Subtotal minus the
///    merchandise discount reaches it; otherwise the quoted zone or base fee.
/// 4. Total = Subtotal + ShippingFee - Discount.
/// Whole đồng only (no double); the shipping fee and threshold are validated whole at the source.
/// </summary>
public static class CartPricing
{
    public static CartTotals Calculate(
        decimal subtotal,
        bool hasPayableLines,
        ShippingQuote shipping,
        decimal? freeShippingThreshold)
    {
        const decimal merchandiseDiscount = 0m;   // coupons plug in here in Phase 3
        const decimal shippingDiscount = 0m;

        var discountedSubtotal = subtotal - merchandiseDiscount;

        var shippingFee = !hasPayableLines
            ? 0m
            : freeShippingThreshold is { } threshold && discountedSubtotal >= threshold
                ? 0m
                : shipping.Fee;

        // A zone with a 0 fee is already free: there is nothing to "unlock" with more shopping.
        decimal? remaining = hasPayableLines && shipping.Fee > 0m && freeShippingThreshold is { } t && discountedSubtotal < t
            ? t - discountedSubtotal
            : null;

        var discount = merchandiseDiscount + shippingDiscount;
        return new CartTotals(subtotal, shippingFee, discount, subtotal + shippingFee - discount, remaining);
    }
}
