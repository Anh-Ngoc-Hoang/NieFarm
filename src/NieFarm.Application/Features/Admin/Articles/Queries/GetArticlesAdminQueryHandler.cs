using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Articles.Dtos;
using NieFarm.Application.Features.Articles.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Articles.Queries;

public class GetArticlesAdminQueryHandler(IReadRepositoryBase<Article> repository)
    : IRequestHandler<GetArticlesAdminQuery, List<ArticleAdminListDto>>
{
    public async Task<List<ArticleAdminListDto>> Handle(
        GetArticlesAdminQuery request, CancellationToken cancellationToken)
    {
        var articles = await repository.ListAsync(new AllArticlesAdminSpec(), cancellationToken);

        return articles.Select(a => new ArticleAdminListDto(
            a.Id,
            a.Title,
            a.Slug,
            a.Category?.Name ?? "—",
            a.Category?.Slug ?? string.Empty,
            a.Author,
            a.ImageUrl,
            a.PublishedOn,
            a.IsPublished)).ToList();
    }
}
