using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>
/// Visible products for the home page's "Sản phẩm nổi bật" band — featured products
/// (<see cref="Product.IsFeatured"/>) first, topped up with the rest of the visible catalog so
/// any catalog with at least <paramref name="take"/> visible products always fills the grid.
/// </summary>
public sealed class HomeFeaturedProductsSpec : Specification<Product>
{
    public HomeFeaturedProductsSpec(int take)
    {
        Query.Where(p => p.IsVisible)
            .Include(p => p.Category)
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Take(take);
    }
}
