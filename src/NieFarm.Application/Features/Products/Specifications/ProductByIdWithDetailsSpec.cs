using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// A product with its full option/variant matrix loaded.
///
/// Every path that writes or reads the matrix must use this rather than
/// <c>GetByIdAsync</c>: the join rows are deleted client-side (see
/// ProductVariantValueConfiguration), so they have to be tracked before a save can remove them.
/// </summary>
public sealed class ProductByIdWithDetailsSpec : Specification<Product>, ISingleResultSpecification<Product>
{
    public ProductByIdWithDetailsSpec(int id)
    {
        Query.Where(p => p.Id == id)
             .Include(p => p.Options).ThenInclude(o => o.Values)
             .Include(p => p.Variants).ThenInclude(v => v.Values).ThenInclude(vv => vv.OptionValue)
             .Include(p => p.Specs)
             .Include(p => p.Features)
             .Include(p => p.Images)
             .AsSplitQuery();
    }
}
