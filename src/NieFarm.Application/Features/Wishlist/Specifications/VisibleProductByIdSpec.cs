using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Wishlist.Specifications;

/// <summary>An existence check for a visible product (no includes, no tracking).</summary>
public sealed class VisibleProductByIdSpec : Specification<Product>
{
    public VisibleProductByIdSpec(int productId)
    {
        Query.AsNoTracking().Where(p => p.Id == productId && p.IsVisible);
    }
}
