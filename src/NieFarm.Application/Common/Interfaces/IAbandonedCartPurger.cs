using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;

namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Deletes guest carts matching <paramref name="spec"/> in small batches (set-based, each batch re-checks
/// the specification inside the DELETE). Implemented in Infrastructure with a fresh DbContext per run.
/// </summary>
public interface IAbandonedCartPurger
{
    Task<CartPurgeResult> PurgeAsync(
        AbandonedGuestCartsSpec spec,
        int batchSize,
        int maxBatches,
        CancellationToken cancellationToken);
}
