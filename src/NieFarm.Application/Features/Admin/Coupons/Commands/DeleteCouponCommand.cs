using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Exceptions;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Coupons.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Coupons.Commands;

/// <summary>Deletes a coupon that has never been redeemed. The FK from Orders (Restrict) is the backstop.</summary>
public record DeleteCouponCommand(int Id) : IRequest<Result>;

public class DeleteCouponCommandHandler(
    IRepositoryBase<Coupon> repository,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<DeleteCouponCommand, Result>
{
    public async Task<Result> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        contextRefresher.DiscardTrackedEntities();

        var coupon = await repository.FirstOrDefaultAsync(new CouponByIdSpec(request.Id, tracked: true), cancellationToken);
        if (coupon is null)
            return Result.NotFound();

        if (coupon.TimesRedeemed > 0)
            return Result.Error(CouponAdminMessages.DeleteUsed);

        try
        {
            await repository.DeleteAsync(coupon, cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Conflict(CouponAdminMessages.Conflict);
        }

        return Result.Success();
    }
}
