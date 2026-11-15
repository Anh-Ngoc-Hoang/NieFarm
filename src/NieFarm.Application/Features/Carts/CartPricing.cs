using NieFarm.Application.Features.Shipping;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Carts;

/// <summary>The result of <see cref="CartPricing.Calculate"/>: every money value the cart shows or an order charges.</summary>
public record CartTotals(
    decimal Subtotal,
    decimal ShippingFee,
    decimal MerchandiseDiscount,
    decimal ShippingDiscount,
    decimal Discount,
    decimal Total,
    decimal? FreeShippingRemaining);

/// <summary>
/// An ELIGIBLE coupon reduced to what the calculator needs. Eligibility (dates, limits, minimum) is decided
/// before this exists, by <see cref="Coupon.CheckEligibility"/>.
/// </summary>
public record CouponQuote(CouponType Type, decimal Value, decimal? MaxDiscountAmount)
{
    public bool WaivesShipping => Type == CouponType.FreeShipping;

    public static CouponQuote From(Coupon coupon) => new(coupon.Type, coupon.Value, coupon.MaxDiscountAmount);
}

/// <summary>
/// The single pure calculator where money is combined (docs/plans/R-09-10-11-cart-coupons-shipping.md D4).
/// Only CartAssembler calls it, and both display and charge go through CartAssembler, so the two can
/// never disagree. Order of operations:
/// 1. Subtotal = sum of the available lines (passed in).
/// 2. Merchandise discount (an eligible Percentage/FixedAmount coupon): the percentage rounded down to a
///    whole đồng, capped by the coupon's maximum, never more than the subtotal.
/// 3. Shipping fee: 0 with no payable lines; 0 when a threshold is set and Subtotal minus the
///    merchandise discount reaches it; otherwise the quoted zone or base fee.
/// 4. Shipping discount: the whole fee for an eligible FreeShipping coupon.
/// 5. Total = Subtotal + ShippingFee - (merchandise + shipping discount).
/// Whole đồng only (no double); the shipping fee and threshold are validated whole at the source.
/// </summary>
public static class CartPricing
{
    public static CartTotals Calculate(
        decimal subtotal,
        bool hasPayableLines,
        ShippingQuote shipping,
        decimal? freeShippingThreshold,
        CouponQuote? coupon)
    {
        var merchandiseDiscount = hasPayableLines && coupon is { WaivesShipping: false }
            ? Coupon.ComputeMerchandiseDiscount(coupon.Type, coupon.Value, coupon.MaxDiscountAmount, subtotal)
            : 0m;

        var discountedSubtotal = subtotal - merchandiseDiscount;

        var shippingFee = !hasPayableLines
            ? 0m
            : freeShippingThreshold is { } threshold && discountedSubtotal >= threshold
                ? 0m
                : shipping.Fee;

        // A FreeShipping coupon on an order the threshold already made free discounts nothing.
        var shippingDiscount = hasPayableLines && coupon is { WaivesShipping: true } ? shippingFee : 0m;

        // A zone with a 0 fee is already free, and a FreeShipping coupon already waives the fee: there is
        // nothing left to "unlock" with more shopping.
        decimal? remaining = hasPayableLines && shipping.Fee > 0m && coupon is not { WaivesShipping: true }
                             && freeShippingThreshold is { } t && discountedSubtotal < t
            ? t - discountedSubtotal
            : null;

        var discount = merchandiseDiscount + shippingDiscount;
        return new CartTotals(
            subtotal, shippingFee, merchandiseDiscount, shippingDiscount, discount,
            subtotal + shippingFee - discount, remaining);
    }
}
