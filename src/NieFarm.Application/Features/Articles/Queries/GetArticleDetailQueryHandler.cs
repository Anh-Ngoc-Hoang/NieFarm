using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Articles.Dtos;
using NieFarm.Application.Features.Articles.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Queries;

public class GetArticleDetailQueryHandler(
    IReadRepositoryBase<Article> repository,
    IHtmlSanitizer htmlSanitizer)
    : IRequestHandler<GetArticleDetailQuery, ArticleDetailDto?>
{
    private const int MaxSlugLength = 270;
    private const int RelatedCount = 3;

    public async Task<ArticleDetailDto?> Handle(
        GetArticleDetailQuery request, CancellationToken cancellationToken)
    {
        var slug = (request.Slug ?? string.Empty).Trim().ToLowerInvariant();
        if (slug.Length == 0 || slug.Length > MaxSlugLength)
            return null;

        var article = await repository.FirstOrDefaultAsync(
            new PublishedArticleBySlugSpec(slug), cancellationToken);
        if (article is null)
            return null;

        var related = await repository.ListAsync(
            new RelatedArticlesSpec(article.CategoryId, article.Id, RelatedCount), cancellationToken);

        string? heroUrl;
        string heroAlt;
        if (!string.IsNullOrWhiteSpace(article.HeroImageUrl))
        {
            heroUrl = article.HeroImageUrl;
            heroAlt = !string.IsNullOrWhiteSpace(article.HeroImageAlt) ? article.HeroImageAlt : article.Title;
        }
        else if (!string.IsNullOrWhiteSpace(article.ImageUrl))
        {
            heroUrl = article.ImageUrl;
            heroAlt = !string.IsNullOrWhiteSpace(article.ImageAlt) ? article.ImageAlt : article.Title;
        }
        else
        {
            heroUrl = null;
            heroAlt = article.Title;
        }

        return new ArticleDetailDto(
            article.Id,
            article.Slug,
            article.Title,
            article.Category!.Name,
            article.Category.Slug,
            article.Author,
            article.PublishedOn,
            article.Summary,
            heroUrl,
            heroAlt,
            // Sanitised on read as well as on write: legacy rows came from a raw textarea.
            htmlSanitizer.Sanitize(article.Body),
            related);
    }
}
