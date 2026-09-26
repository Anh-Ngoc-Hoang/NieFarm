using MediatR;
using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Products.Queries;

public record GetRelatedProductsQuery(int ExcludeProductId, int Take = 4)
    : IRequest<IReadOnlyList<ProductCardDto>>;
