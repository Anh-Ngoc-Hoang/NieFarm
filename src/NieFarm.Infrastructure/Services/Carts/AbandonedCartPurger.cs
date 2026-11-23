using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Carts.Dtos;
using NieFarm.Application.Features.Carts.Specifications;
using NieFarm.Infrastructure.Data;

namespace NieFarm.Infrastructure.Services.Carts;

/// <summary>
/// Deletes abandoned guest carts (R-23) in small batches so no single statement holds locks for long.
/// Uses a fresh DbContext per run (the factory is scoped, so the caller supplies its own scope) and
/// set-based statements instead of loading and tracking entities.
///
/// Each batch: (1) pick up to batchSize cart ids matching the specification, (2) count their items for
/// the log, (3) DELETE those ids with the WHOLE specification applied again. Step 3 re-checks every
/// condition, including <c>UserId IS NULL</c>, so a cart that was edited or claimed by an account
/// between steps 1 and 3 is not deleted. CartItems go with their cart through the ON DELETE CASCADE foreign key.
/// Exceptions are not caught here: the handler's caller logs them and retries on the next cycle.
/// Nothing in here logs a cart id, an anonymous id or a coupon code.
/// </summary>
internal sealed class AbandonedCartPurger(IDbContextFactory<AppDbContext> contextFactory) : IAbandonedCartPurger
{
    private static readonly TimeSpan PauseBetweenBatches = TimeSpan.FromMilliseconds(200);

    public async Task<CartPurgeResult> PurgeAsync(
        AbandonedGuestCartsSpec spec,
        int batchSize,
        int maxBatches,
        CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var cartsDeleted = 0;
        var itemsDeleted = 0;
        var batches = 0;
        var moreRemaining = false;

        while (batches < maxBatches)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ids = await db.Carts
                .WithSpecification(spec)
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (ids.Count == 0)
                break;

            // Count only drives the log line; a concurrent edit can make it slightly off.
            var items = await db.Carts
                .WithSpecification(spec)
                .Where(c => ids.Contains(c.Id))
                .SelectMany(c => c.Items)
                .CountAsync(cancellationToken);

            var deleted = await db.Carts
                .WithSpecification(spec)
                .Where(c => ids.Contains(c.Id))
                .ExecuteDeleteAsync(cancellationToken);

            batches++;
            cartsDeleted += deleted;
            itemsDeleted += items;

            if (ids.Count < batchSize)
                break;

            // A full batch means there may be more; if we are out of batches say so.
            if (batches >= maxBatches)
            {
                moreRemaining = true;
                break;
            }

            await Task.Delay(PauseBetweenBatches, cancellationToken);
        }

        return new CartPurgeResult(cartsDeleted, itemsDeleted, batches, moreRemaining);
    }
}
