using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// Visible products by id, with just enough of the option/variant matrix loaded to re-derive a
/// cart line's name/price/image/availability. No Images / Specs / Features include — the cart
/// needs Name, ImageUrl, ImageAlt, Price and the variant matrix only.
/// </summary>
public sealed class ProductsByIdsForCartSpec : Specification<Product>
{
    public ProductsByIdsForCartSpec(IReadOnlyCollection<int> ids)
    {
        Query.Where(p => ids.Contains(p.Id) && p.IsVisible)
            .Include(p => p.Options).ThenInclude(o => o.Values)
            .Include(p => p.Variants).ThenInclude(v => v.Values).ThenInclude(vv => vv.OptionValue)
            .AsSplitQuery();
    }
}
