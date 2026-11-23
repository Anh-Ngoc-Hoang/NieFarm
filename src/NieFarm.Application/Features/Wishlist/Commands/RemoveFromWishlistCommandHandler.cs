using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Wishlist.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Commands;

public class RemoveFromWishlistCommandHandler(
    IRepositoryBase<WishlistItem> repository,
    IWriteContextRefresher refresher,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveFromWishlistCommand, Result>
{
    public async Task<Result> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();

        refresher.DiscardTrackedEntities();

        // The owner is part of the WHERE: another customer's row is never found, so it is never deleted.
        var item = await repository.FirstOrDefaultAsync(new WishlistItemSpec(userId, request.ProductId), cancellationToken);
        if (item is not null)
            await repository.DeleteAsync(item, cancellationToken);

        return Result.Success();
    }
}
