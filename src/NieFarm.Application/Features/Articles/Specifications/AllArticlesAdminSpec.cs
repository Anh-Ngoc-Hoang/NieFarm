using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Specifications;

/// <summary>Every article, published or not, newest first, with its category loaded.</summary>
public sealed class AllArticlesAdminSpec : Specification<Article>
{
    public AllArticlesAdminSpec()
    {
        Query.Include(a => a.Category)
             .OrderByDescending(a => a.PublishedOn)
             .ThenByDescending(a => a.Id);
    }
}
