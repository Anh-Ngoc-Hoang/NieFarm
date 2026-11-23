using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Specifications;

/// <summary>
/// The owner's wishlist rows, most recently added first (R-16 D-23). No-tracking, because the read
/// repository can live as long as the circuit. Ownership is part of the WHERE; at most
/// <see cref="WishlistItem.MaxPerUser"/> rows exist, so there is no paging.
/// </summary>
public sealed class WishlistByUserSpec : Specification<WishlistItem>
{
    public WishlistByUserSpec(string userId)
    {
        Query.AsNoTracking()
             .Where(w => w.UserId == userId)
             .OrderByDescending(w => w.CreatedAt)
             .ThenByDescending(w => w.Id);
    }
}
