using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Specifications;

/// <summary>
/// The single authority on public visibility for one article: published and in a visible category.
/// Anything else (unknown, draft, hidden category) must be a 404.
/// </summary>
public sealed class PublishedArticleBySlugSpec : Specification<Article>, ISingleResultSpecification<Article>
{
    public PublishedArticleBySlugSpec(string slug)
    {
        Query.Where(a => a.Slug == slug && a.IsPublished && a.Category!.IsVisible)
             .Include(a => a.Category);
    }
}
