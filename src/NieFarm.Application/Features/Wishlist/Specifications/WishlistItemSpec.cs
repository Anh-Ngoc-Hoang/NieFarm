using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Specifications;

/// <summary>
/// One row by (owner, product), tracked, for the write handlers. The owner is part of the WHERE
/// (R-16 D-14 style), so another customer's row can never be found or removed through it.
/// </summary>
public sealed class WishlistItemSpec : Specification<WishlistItem>, ISingleResultSpecification<WishlistItem>
{
    public WishlistItemSpec(string userId, int productId)
    {
        Query.Where(w => w.UserId == userId && w.ProductId == productId);
    }
}
