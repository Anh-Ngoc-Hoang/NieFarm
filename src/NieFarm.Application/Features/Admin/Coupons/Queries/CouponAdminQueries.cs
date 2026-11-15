using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Coupons.Dtos;
using NieFarm.Application.Features.Coupons.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Coupons.Queries;

public record GetCouponsAdminQuery : IRequest<List<CouponAdminListDto>>;

public record GetCouponForEditQuery(int Id) : IRequest<CouponEditDto?>;

public class GetCouponsAdminQueryHandler(
    IReadRepositoryBase<Coupon> couponRepository,
    IReadRepositoryBase<Order> orderRepository)
    : IRequestHandler<GetCouponsAdminQuery, List<CouponAdminListDto>>
{
    public async Task<List<CouponAdminListDto>> Handle(
        GetCouponsAdminQuery request, CancellationToken cancellationToken)
    {
        var coupons = await couponRepository.ListAsync(new AllCouponsAdminSpec(), cancellationToken);
        if (coupons.Count == 0)
            return [];

        // One row per active use, grouped here: no order entity is loaded.
        var useRows = await orderRepository.ListAsync(
            new CouponUsageByCouponSpec(coupons.Select(c => c.Id).ToList()), cancellationToken);
        var uses = useRows.Where(id => id is not null).GroupBy(id => id!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var now = DateTime.UtcNow;
        return coupons.Select(c =>
        {
            var active = uses.GetValueOrDefault(c.Id);
            return new CouponAdminListDto(
                c.Id, c.Code, c.Description, c.Type, c.Value, c.MaxDiscountAmount, c.MinOrderSubtotal,
                c.StartsAt, c.EndsAt, c.UsageLimit, c.PerCustomerLimit, c.IsActive, c.TimesRedeemed, active,
                CouponAdminStatusResolver.Resolve(c, active, now));
        }).ToList();
    }
}

public class GetCouponForEditQueryHandler(IReadRepositoryBase<Coupon> repository)
    : IRequestHandler<GetCouponForEditQuery, CouponEditDto?>
{
    public async Task<CouponEditDto?> Handle(GetCouponForEditQuery request, CancellationToken cancellationToken)
    {
        var c = await repository.FirstOrDefaultAsync(new CouponByIdSpec(request.Id), cancellationToken);
        return c is null
            ? null
            : new CouponEditDto(
                c.Id, c.Code, c.Description, c.Type, c.Value, c.MaxDiscountAmount, c.MinOrderSubtotal,
                c.StartsAt, c.EndsAt, c.UsageLimit, c.PerCustomerLimit, c.IsActive, c.TimesRedeemed);
    }
}
