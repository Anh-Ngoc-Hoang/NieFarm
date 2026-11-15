using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Coupons.Specifications;

/// <summary>
/// The active uses of a coupon: orders carrying it that are not cancelled. The order row is the
/// redemption, so cancelling an order frees the use with no release step (R-10 D14). Used with
/// <c>CountAsync</c>; backed by IX_Orders_CouponId_Status.
/// </summary>
public sealed class ActiveCouponOrdersSpec : Specification<Order>
{
    public ActiveCouponOrdersSpec(int couponId)
    {
        Query.Where(o => o.CouponId == couponId && o.Status != OrderStatus.Cancelled)
            .AsNoTracking();
    }
}
