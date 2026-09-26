using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Queries;

public class GetShopCategoriesQueryHandler(IReadRepositoryBase<ProductCategory> repository)
    : IRequestHandler<GetShopCategoriesQuery, IReadOnlyList<ProductCategoryFilterDto>>
{
    public async Task<IReadOnlyList<ProductCategoryFilterDto>> Handle(
        GetShopCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new VisibleProductCategoriesSpec(), cancellationToken);

        return categories
            .Select(c => new ProductCategoryFilterDto(c.Id, c.Name))
            .ToList();
    }
}
