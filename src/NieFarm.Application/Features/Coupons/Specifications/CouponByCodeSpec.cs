using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Coupons.Specifications;

/// <summary>
/// A coupon by its (already normalised) code. Read-side callers get an untracked row, because the
/// read repository behind <c>CartAssembler</c> lives as long as the circuit and a tracked query would
/// keep returning the first-loaded coupon, hiding an admin edit. The redemption flow passes
/// <c>tracked: true</c> through the WRITE repository so the row version is checked on save.
/// </summary>
public sealed class CouponByCodeSpec : Specification<Coupon>, ISingleResultSpecification<Coupon>
{
    public CouponByCodeSpec(string code, bool tracked = false)
    {
        Query.Where(c => c.Code == code);

        if (!tracked)
            Query.AsNoTracking();
    }
}
