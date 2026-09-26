using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// A single visible product with its option/variant matrix loaded, for the cart write path
/// (AddToCartCommand / SetCartLineQuantityCommand). Do not reuse ProductByIdWithDetailsSpec — it
/// has no IsVisible filter, because it serves the admin editor.
/// </summary>
public sealed class ProductByIdForCartSpec : Specification<Product>, ISingleResultSpecification<Product>
{
    public ProductByIdForCartSpec(int id)
    {
        Query.Where(p => p.Id == id && p.IsVisible)
            .Include(p => p.Options).ThenInclude(o => o.Values)
            .Include(p => p.Variants).ThenInclude(v => v.Values).ThenInclude(vv => vv.OptionValue)
            .AsSplitQuery();
    }
}
