using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Coupons.Specifications;

/// <summary>Every coupon, newest first.</summary>
public sealed class AllCouponsAdminSpec : Specification<Coupon>
{
    public AllCouponsAdminSpec()
    {
        Query.OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .AsNoTracking();
    }
}
