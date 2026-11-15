using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Coupons.Specifications;

/// <summary>
/// Active uses of a coupon by one customer: by account id OR by the phone key typed on the order, so a
/// signed-in user is counted by either and a guest by phone. With neither identity it matches nothing
/// (callers pass null counts in that case instead of querying). Guests rotating phone numbers can evade
/// the limit; that is inherent and documented in the plan (D16).
/// </summary>
public sealed class ActiveCouponOrdersForCustomerSpec : Specification<Order>
{
    public ActiveCouponOrdersForCustomerSpec(int couponId, string? userId, string? phoneKey)
    {
        Query.Where(o => o.CouponId == couponId && o.Status != OrderStatus.Cancelled)
            .AsNoTracking();

        var hasUser = !string.IsNullOrWhiteSpace(userId);
        var hasPhone = !string.IsNullOrWhiteSpace(phoneKey);

        if (hasUser && hasPhone)
            Query.Where(o => o.CustomerId == userId || o.CouponPhoneKey == phoneKey);
        else if (hasUser)
            Query.Where(o => o.CustomerId == userId);
        else if (hasPhone)
            Query.Where(o => o.CouponPhoneKey == phoneKey);
        else
            Query.Where(o => false);
    }
}
