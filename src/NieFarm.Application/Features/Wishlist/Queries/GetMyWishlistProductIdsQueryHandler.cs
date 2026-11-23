using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Wishlist.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Queries;

public class GetMyWishlistProductIdsQueryHandler(
    IReadRepositoryBase<WishlistItem> repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyWishlistProductIdsQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> Handle(
        GetMyWishlistProductIdsQuery request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return [];

        var rows = await repository.ListAsync(new WishlistByUserSpec(userId), cancellationToken);
        return rows.Select(w => w.ProductId).ToList();
    }
}
