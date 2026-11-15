using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Coupons.Dtos;

/// <summary>The admin list status, computed from the dates, the limit and the active uses (not stored).</summary>
public enum CouponAdminStatus
{
    Active,
    Scheduled,
    Expired,
    UsedUp,
    Disabled
}

public record CouponAdminListDto(
    int Id,
    string Code,
    string? Description,
    CouponType Type,
    decimal Value,
    decimal? MaxDiscountAmount,
    decimal MinOrderSubtotal,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    int? UsageLimit,
    int? PerCustomerLimit,
    bool IsActive,
    int TimesRedeemed,
    int ActiveUses,
    CouponAdminStatus Status);

public record CouponEditDto(
    int Id,
    string Code,
    string? Description,
    CouponType Type,
    decimal Value,
    decimal? MaxDiscountAmount,
    decimal MinOrderSubtotal,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    int? UsageLimit,
    int? PerCustomerLimit,
    bool IsActive,
    int TimesRedeemed);

public static class CouponAdminStatusResolver
{
    public static CouponAdminStatus Resolve(Coupon coupon, int activeUses, DateTime nowUtc)
    {
        if (!coupon.IsActive) return CouponAdminStatus.Disabled;
        if (coupon.StartsAt is { } from && nowUtc < from) return CouponAdminStatus.Scheduled;
        if (coupon.EndsAt is { } to && nowUtc >= to) return CouponAdminStatus.Expired;
        if (coupon.UsageLimit is { } limit && activeUses >= limit) return CouponAdminStatus.UsedUp;
        return CouponAdminStatus.Active;
    }
}
