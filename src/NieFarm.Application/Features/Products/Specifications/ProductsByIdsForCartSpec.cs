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
            .AsSplitQuery()
            // CartAssembler is scoped, so its read repository (one context) lives as long as the
            // circuit. A tracked query would keep returning the first-loaded Product, and a price or
            // availability edit would never reach the cart or the ExpectedTotal check. No tracking
            // means every build reads the database; identity resolution keeps shared option values
            // as one instance.
            .AsNoTrackingWithIdentityResolution();
    }
}
