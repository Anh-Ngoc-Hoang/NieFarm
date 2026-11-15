using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Articles.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Articles.Queries;

public class GetArticleForEditQueryHandler(IReadRepositoryBase<Article> repository, IHtmlSanitizer htmlSanitizer)
    : IRequestHandler<GetArticleForEditQuery, ArticleEditDto?>
{
    public async Task<ArticleEditDto?> Handle(
        GetArticleForEditQuery request, CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (article is null)
            return null;

        return new ArticleEditDto(
            article.Id,
            article.Title,
            article.Slug,
            article.CategoryId,
            article.Author,
            article.Summary,
            article.ImageUrl,
            article.ImageAlt,
            article.HeroImageUrl,
            article.HeroImageAlt,
            // Rows saved through the old raw textarea were never sanitised.
            htmlSanitizer.Sanitize(article.Body),
            article.PublishedOn,
            article.IsPublished);
    }
}
