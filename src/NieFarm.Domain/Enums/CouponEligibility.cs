namespace NieFarm.Domain.Enums;

/// <summary>The outcome of <c>Coupon.CheckEligibility</c>. Not persisted.</summary>
public enum CouponEligibility
{
    Eligible,
    Inactive,
    NotStarted,
    Expired,
    UsedUp,
    BelowMinimum,
    CustomerLimitReached
}
