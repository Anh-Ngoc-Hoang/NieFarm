using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Queries;

public class GetHomeFeaturedProductsQueryHandler(
    IReadRepositoryBase<Product> repository,
    IReadRepositoryBase<Review> reviews)
    : IRequestHandler<GetHomeFeaturedProductsQuery, IReadOnlyList<ProductCardDto>>
{
    public async Task<IReadOnlyList<ProductCardDto>> Handle(
        GetHomeFeaturedProductsQuery request, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 8);

        var products = await repository.ListAsync(new HomeFeaturedProductsSpec(take), cancellationToken);

        return await ProductCardAssembler.ToCardsAsync(products, reviews, cancellationToken);
    }
}
