using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// Other visible products for the "Sản phẩm liên quan" band — no category-matching or
/// personalisation yet, just the next <paramref name="take"/> visible products by display
/// order, excluding the one currently being viewed.
/// </summary>
public sealed class RelatedProductsSpec : Specification<Product>
{
    public RelatedProductsSpec(int excludeProductId, int take)
    {
        Query.Where(p => p.IsVisible && p.Id != excludeProductId)
            .Include(p => p.Category)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .Take(take);
    }
}
