using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Specifications;

public sealed class VisibleArticleCategoriesSpec : Specification<ArticleCategory>
{
    public VisibleArticleCategoriesSpec()
    {
        Query.Where(c => c.IsVisible)
             .OrderBy(c => c.SortOrder)
             .ThenBy(c => c.Id);
    }
}
