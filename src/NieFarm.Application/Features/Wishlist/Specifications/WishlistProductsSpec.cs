using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Specifications;

/// <summary>
/// The visible products among a user's wishlist ids, loaded the way <c>ProductCardMapper</c> needs
/// (Category, Options, Variants with values). A hidden product is simply absent: its wishlist row is
/// kept but not shown. No-tracking, because the read repository can live as long as the circuit.
/// </summary>
public sealed class WishlistProductsSpec : Specification<Product>
{
    public WishlistProductsSpec(IReadOnlyCollection<int> ids)
    {
        Query.Where(p => ids.Contains(p.Id) && p.IsVisible)
            .Include(p => p.Category)
            .Include(p => p.Options).ThenInclude(o => o.Values)
            .Include(p => p.Variants).ThenInclude(v => v.Values)
            .AsSplitQuery()
            .AsNoTrackingWithIdentityResolution();
    }
}
