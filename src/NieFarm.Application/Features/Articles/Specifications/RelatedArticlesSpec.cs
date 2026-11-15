using Ardalis.Specification;
using NieFarm.Application.Features.Articles.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Specifications;

/// <summary>Newest published articles of the same (visible) category, excluding the current one.</summary>
public sealed class RelatedArticlesSpec : Specification<Article, ArticleCardDto>
{
    public RelatedArticlesSpec(int categoryId, int excludeId, int take)
    {
        Query.Where(a => a.IsPublished && a.Category!.IsVisible
                         && a.CategoryId == categoryId && a.Id != excludeId)
             .OrderByDescending(a => a.PublishedOn)
             .ThenByDescending(a => a.Id)
             .Take(take);

        Query.Select(a => new ArticleCardDto(
            a.Id,
            a.Slug,
            a.Title,
            a.Category!.Name,
            a.PublishedOn,
            a.Summary,
            a.ImageUrl != "" ? a.ImageUrl : (a.HeroImageUrl != "" ? a.HeroImageUrl : null),
            a.ImageUrl != ""
                ? (a.ImageAlt != "" ? a.ImageAlt : a.Title)
                : (a.HeroImageUrl != "" && a.HeroImageAlt != "" ? a.HeroImageAlt : a.Title)));
    }
}
