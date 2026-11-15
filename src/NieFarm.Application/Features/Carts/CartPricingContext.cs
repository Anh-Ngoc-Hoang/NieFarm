using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Carts;

/// <summary>
/// The pricing inputs that are not part of the cart itself.
/// <list type="bullet">
/// <item><description><b>Province</b>: the name the shopper chose or typed; a pricing input only, never stored on the cart.</description></item>
/// <item><description><b>UserId / PhoneKey</b>: who is asking, so a coupon's per-customer limit can be evaluated.
/// Display paths know only the account; the order handler also knows the phone typed at checkout.</description></item>
/// <item><description><b>TrackedCoupon</b>: the coupon row loaded through the WRITE repository by the order
/// handler, so the redemption's UPDATE is checked against its row version. The coupon must be loaded before
/// the assembler counts uses (docs plan D15).</description></item>
/// <item><description><b>CouponCodeOverride</b>: evaluate this code instead of the cart's own (the apply command
/// tries a code before it stores it).</description></item>
/// </list>
/// </summary>
public record CartPricingContext(
    string? Province,
    string? UserId = null,
    string? PhoneKey = null,
    Coupon? TrackedCoupon = null,
    string? CouponCodeOverride = null)
{
    public static CartPricingContext None { get; } = new((string?)null);
}
