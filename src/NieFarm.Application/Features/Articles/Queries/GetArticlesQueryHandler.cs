using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Articles.Dtos;
using NieFarm.Application.Features.Articles.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Queries;

public class GetArticlesQueryHandler(IReadRepositoryBase<Article> repository)
    : IRequestHandler<GetArticlesQuery, IReadOnlyList<ArticleCardDto>>
{
    private const int MaxSearchLength = 100;

    public async Task<IReadOnlyList<ArticleCardDto>> Handle(
        GetArticlesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim();
        if (string.IsNullOrEmpty(search))
            search = null;
        else if (search.Length > MaxSearchLength)
            search = search[..MaxSearchLength];

        return await repository.ListAsync(
            new PublishedArticlesSpec(request.CategoryId, search), cancellationToken);
    }
}
