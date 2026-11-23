using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Wishlist.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Commands;

/// <summary>
/// The write path follows the R-07 pattern: ICurrentUser for the owner, DiscardTrackedEntities so the
/// duplicate and limit checks run against database state (the write context lives as long as the
/// circuit), the write repository for both the check and the insert. The product itself is only an
/// existence check, read through a fresh read context and then discarded (data-access.md).
/// </summary>
public class AddToWishlistCommandHandler(
    IRepositoryBase<WishlistItem> repository,
    IReadRepositoryBase<Product> products,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser)
    : IRequestHandler<AddToWishlistCommand, Result>
{
    public async Task<Result> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();

        if (!await products.AnyAsync(new VisibleProductByIdSpec(request.ProductId), cancellationToken))
            return Result.NotFound(WishlistMessages.ProductNotFound);

        refresher.DiscardTrackedEntities();

        // Already saved: idempotent success, nothing is inserted.
        if (await repository.AnyAsync(new WishlistItemSpec(userId, request.ProductId), cancellationToken))
            return Result.Success();

        if (await repository.CountAsync(new WishlistByUserSpec(userId), cancellationToken) >= WishlistItem.MaxPerUser)
            return Result.Error(WishlistMessages.LimitReached);

        try
        {
            await repository.AddAsync(WishlistItem.Create(userId, request.ProductId), cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Two tabs (or a double click that slipped past the UI guard) raced on the unique
            // (UserId, ProductId) index. Application has no EF reference, so the DbUpdateException
            // arrives as a plain exception; the repository has already cleared the tracker. If the row
            // is there now, the goal is met; otherwise it was a genuine failure and surfaces as such.
            var exists = false;
            try
            {
                exists = await repository.AnyAsync(new WishlistItemSpec(userId, request.ProductId), cancellationToken);
            }
            catch (Exception)
            {
                // The re-check failed too: keep the ORIGINAL exception (below), and never claim success.
            }

            if (exists)
                return Result.Success();
            throw;
        }

        return Result.Success();
    }
}
