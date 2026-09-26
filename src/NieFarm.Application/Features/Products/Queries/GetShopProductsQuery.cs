using MediatR;
using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Products.Queries;

/// <summary>
/// An empty or null <paramref name="CategoryIds"/> means "no filter" — show every visible
/// product — never "no results".
/// </summary>
public record GetShopProductsQuery(
    IReadOnlyCollection<int>? CategoryIds = null,
    ShopSortOrder Sort = ShopSortOrder.Newest) : IRequest<IReadOnlyList<ProductCardDto>>;
