using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;

namespace NieFarm.Application.Features.Carts.Commands;

public class PurgeAbandonedCartsCommandHandler(IAbandonedCartPurger purger)
    : IRequestHandler<PurgeAbandonedCartsCommand, Result<CartPurgeResult>>
{
    public async Task<Result<CartPurgeResult>> Handle(
        PurgeAbandonedCartsCommand request, CancellationToken cancellationToken)
    {
        // One clock reading so every threshold is measured from the same instant.
        var now = DateTime.UtcNow;

        var spec = new AbandonedGuestCartsSpec(
            unreachableBeforeUtc: now - CartRetentionPolicy.UnreachableAfter,
            emptyIdleBeforeUtc: now - TimeSpan.FromDays(request.EmptyCartIdleDays),
            idleBeforeUtc: request.IdleCartDays is > 0 ? now - TimeSpan.FromDays(request.IdleCartDays.Value) : null);

        var result = await purger.PurgeAsync(spec, request.BatchSize, request.MaxBatches, cancellationToken);

        return Result.Success(result);
    }
}
