using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// A visible product with everything /san-pham/{slug} needs, keyed by slug. Slug comparison
/// stays "==", matching ProductBySlugSpec — the DB collation is case-insensitive and
/// Product.Update lower-cases slugs on write.
/// </summary>
public sealed class ProductDetailBySlugSpec : Specification<Product>, ISingleResultSpecification<Product>
{
    public ProductDetailBySlugSpec(string slug)
    {
        Query.Where(p => p.Slug == slug && p.IsVisible)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Options).ThenInclude(o => o.Values)
            .Include(p => p.Variants).ThenInclude(v => v.Values).ThenInclude(vv => vv.OptionValue)
            .Include(p => p.Specs)
            .Include(p => p.Features)
            .AsSplitQuery();
    }
}
