using Ardalis.Specification;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

public sealed class VisibleProductsSpec : Specification<Product>
{
    public VisibleProductsSpec() : this(null, ShopSortOrder.Newest)
    {
    }

    public VisibleProductsSpec(IReadOnlyCollection<int>? categoryIds, ShopSortOrder sort)
    {
        Query.Where(p => p.IsVisible);

        // An empty or null set means "no filter", not "no results".
        if (categoryIds is { Count: > 0 })
            Query.Where(p => categoryIds.Contains(p.CategoryId));

        Query.Include(p => p.Category);

        // Exactly one ordering chain — two unconditional OrderBy calls on the same builder
        // would silently discard the first.
        switch (sort)
        {
            case ShopSortOrder.PriceAscending:
                Query.OrderBy(p => p.Price).ThenBy(p => p.SortOrder);
                break;
            case ShopSortOrder.PriceDescending:
                Query.OrderByDescending(p => p.Price).ThenBy(p => p.SortOrder);
                break;
            case ShopSortOrder.Newest:
            default:
                Query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);
                break;
        }
    }
}
