using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Coupons.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Coupons.Commands;

/// <summary>Enables or disables a coupon without touching anything else. Idempotent.</summary>
public record SetCouponActiveCommand(int Id, bool IsActive) : IRequest<Result>;

public class SetCouponActiveCommandHandler(
    IRepositoryBase<Coupon> repository,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<SetCouponActiveCommand, Result>
{
    public async Task<Result> Handle(SetCouponActiveCommand request, CancellationToken cancellationToken)
    {
        contextRefresher.DiscardTrackedEntities();

        var coupon = await repository.FirstOrDefaultAsync(new CouponByIdSpec(request.Id, tracked: true), cancellationToken);
        if (coupon is null)
            return Result.NotFound();

        if (coupon.IsActive == request.IsActive)
            return Result.Success();

        try
        {
            coupon.SetActive(request.IsActive);
            await repository.UpdateAsync(coupon, cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Conflict(CouponAdminMessages.Conflict);
        }

        return Result.Success();
    }
}
