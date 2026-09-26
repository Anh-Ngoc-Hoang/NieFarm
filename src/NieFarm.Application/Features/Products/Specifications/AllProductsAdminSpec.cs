using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

/// <summary>Every product, visible or not, with its category loaded, in display order.</summary>
public sealed class AllProductsAdminSpec : Specification<Product>
{
    public AllProductsAdminSpec()
    {
        // Variants only — the list page shows a price range and a count, never the labels.
        Query.Include(p => p.Category)
             .Include(p => p.Variants)
             .AsSplitQuery()
             .OrderBy(p => p.SortOrder)
             .ThenBy(p => p.Id);
    }
}
