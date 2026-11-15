using Ardalis.Specification;
using NieFarm.Application.Features.Articles.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Specifications;

/// <summary>
/// Public listing: published articles in a visible category, newest first, projected to cards so
/// the <c>nvarchar(max)</c> body is never loaded. <c>search</c> must already be trimmed and
/// length-capped by the caller; blank means no filter.
/// </summary>
public sealed class PublishedArticlesSpec : Specification<Article, ArticleCardDto>
{
    public PublishedArticlesSpec(int? categoryId, string? search)
    {
        Query.Where(a => a.IsPublished && a.Category!.IsVisible);

        if (categoryId is { } id)
            Query.Where(a => a.CategoryId == id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search;
            Query.Where(a => a.Title.Contains(term) || a.Summary.Contains(term));
        }

        Query.OrderByDescending(a => a.PublishedOn)
             .ThenByDescending(a => a.Id);

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
