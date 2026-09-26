using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Specifications;

public sealed class ArticleBySlugSpec : Specification<Article>, ISingleResultSpecification<Article>
{
    public ArticleBySlugSpec(string slug, int? excludeId = null)
    {
        Query.Where(a => a.Slug == slug);

        if (excludeId is { } id)
            Query.Where(a => a.Id != id);
    }
}
