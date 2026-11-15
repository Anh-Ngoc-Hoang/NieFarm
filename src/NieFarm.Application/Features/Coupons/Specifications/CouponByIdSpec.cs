using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Coupons.Specifications;

public sealed class CouponByIdSpec : Specification<Coupon>, ISingleResultSpecification<Coupon>
{
    public CouponByIdSpec(int id, bool tracked = false)
    {
        Query.Where(c => c.Id == id);

        if (!tracked)
            Query.AsNoTracking();
    }
}
