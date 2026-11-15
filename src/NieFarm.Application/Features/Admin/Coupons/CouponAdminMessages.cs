namespace NieFarm.Application.Features.Admin.Coupons;

/// <summary>Admin-facing (English) coupon messages.</summary>
public static class CouponAdminMessages
{
    public const string Conflict = "This coupon was just used or edited. Reload and try again.";

    public const string DeleteUsed = "A coupon that has been used cannot be deleted. Disable it instead.";
}
