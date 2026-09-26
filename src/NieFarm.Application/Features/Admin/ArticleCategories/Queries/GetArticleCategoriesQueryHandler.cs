using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.ArticleCategories.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.ArticleCategories.Queries;

public class GetArticleCategoriesQueryHandler(
    IReadRepositoryBase<ArticleCategory> categories,
    IReadRepositoryBase<Article> articles)
    : IRequestHandler<GetArticleCategoriesQuery, List<ArticleCategoryDto>>
{
    public async Task<List<ArticleCategoryDto>> Handle(
        GetArticleCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Both reads go through IReadRepositoryBase, so each gets its own DbContext and
        // they are safe to run in parallel — see .claude/rules/data-access.md.
        var categoryTask = categories.ListAsync(cancellationToken);
        var articleTask = articles.ListAsync(cancellationToken);
        await Task.WhenAll(categoryTask, articleTask);

        var counts = articleTask.Result
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        return categoryTask.Result
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .Select(c => new ArticleCategoryDto(
                c.Id, c.Name, c.Slug, c.IsVisible, c.SortOrder,
                counts.TryGetValue(c.Id, out var count) ? count : 0))
            .ToList();
    }
}
