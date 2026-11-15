using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Coupons.Specifications;

/// <summary>
/// The coupon id of every active (non-cancelled) order that used one of the given coupons, one row per
/// use, so the admin list can show "active / limit" with a single query. Projection only: no order
/// entity is loaded.
/// </summary>
public sealed class CouponUsageByCouponSpec : Specification<Order, int?>
{
    public CouponUsageByCouponSpec(IReadOnlyCollection<int> couponIds)
    {
        Query.Where(o => o.CouponId != null && couponIds.Contains(o.CouponId.Value)
                         && o.Status != OrderStatus.Cancelled)
            .AsNoTracking();
        Query.Select(o => o.CouponId);
    }
}
