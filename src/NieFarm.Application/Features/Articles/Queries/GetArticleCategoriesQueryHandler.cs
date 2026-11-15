using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Articles.Dtos;
using NieFarm.Application.Features.Articles.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Articles.Queries;

public class GetArticleCategoriesQueryHandler(IReadRepositoryBase<ArticleCategory> repository)
    : IRequestHandler<GetArticleCategoriesQuery, IReadOnlyList<ArticleCategoryFilterDto>>
{
    public async Task<IReadOnlyList<ArticleCategoryFilterDto>> Handle(
        GetArticleCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new VisibleArticleCategoriesSpec(), cancellationToken);
        return categories.Select(c => new ArticleCategoryFilterDto(c.Id, c.Name, c.Slug)).ToList();
    }
}
