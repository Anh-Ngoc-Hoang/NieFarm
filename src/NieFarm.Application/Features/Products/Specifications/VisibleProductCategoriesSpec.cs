using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Specifications;

public sealed class VisibleProductCategoriesSpec : Specification<ProductCategory>
{
    public VisibleProductCategoriesSpec()
    {
        Query.Where(c => c.IsVisible)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name);
    }
}
