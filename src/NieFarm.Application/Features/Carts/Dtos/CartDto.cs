using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Carts.Dtos;

public record CartLineDto(
    int ProductId,
    string VariantKey,
    string Slug,          // so the checkout line can link back to /san-pham/{slug}
    string Name,
    string Variant,       // display form, "1kg / Xay pha phin"; empty when the product has no options
    string ImageUrl,
    string ImageAlt,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    bool IsAvailable);

/// <summary>
/// The coupon code the cart remembers and how it was evaluated on this build. <see cref="IsApplied"/> is true
/// only when the coupon is eligible AND actually lowers the total; otherwise <see cref="Message"/> says why
/// (Vietnamese, customer-facing). <see cref="Type"/> is null when the code no longer resolves to a coupon.
/// </summary>
public record CartCouponDto(
    string Code,
    CouponType? Type,
    bool IsApplied,
    string? Message,
    CouponEligibility Eligibility);

public record CartDto(
    IReadOnlyList<CartLineDto> Lines,
    int ItemCount,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Discount,
    decimal Total,
    string? ShippingZoneName = null,
    bool ShippingProvinceResolved = false,
    decimal? FreeShippingThreshold = null,
    decimal? FreeShippingRemaining = null,
    CartCouponDto? Coupon = null)
{
    public bool IsEmpty => Lines.Count == 0;
    public bool HasUnavailableLines => Lines.Any(l => !l.IsAvailable);
    public static CartDto Empty { get; } = new([], 0, 0m, 0m, 0m, 0m);
}
