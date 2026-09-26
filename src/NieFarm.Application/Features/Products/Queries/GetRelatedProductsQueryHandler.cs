using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Queries;

public class GetRelatedProductsQueryHandler(IReadRepositoryBase<Product> repository)
    : IRequestHandler<GetRelatedProductsQuery, IReadOnlyList<ProductCardDto>>
{
    public async Task<IReadOnlyList<ProductCardDto>> Handle(
        GetRelatedProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await repository.ListAsync(
            new RelatedProductsSpec(request.ExcludeProductId, request.Take), cancellationToken);

        return products
            .Select(p => new ProductCardDto(
                p.Slug,
                p.Name,
                p.Category?.Name ?? string.Empty,
                p.Price,
                p.ImageUrl,
                p.ImageAlt,
                string.IsNullOrWhiteSpace(p.Badge) ? null : p.Badge,
                p.Summary))
            .ToList();
    }
}
