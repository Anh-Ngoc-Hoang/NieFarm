using MediatR;
using NieFarm.Application.Common.Models;
using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Products.Queries;

/// <summary>
/// One page of the shop. An empty or null <paramref name="CategoryIds"/> means "no filter" — show
/// every visible product — never "no results". <paramref name="Search"/> is trimmed/normalised by
/// the handler (<see cref="ProductSearchTerm"/>). <paramref name="Page"/> and
/// <paramref name="PageSize"/> are only requests: the handler clamps them and the returned
/// <see cref="PagedResult{T}"/> carries the values actually used.
/// </summary>
public record GetShopProductsQuery(
    IReadOnlyCollection<int>? CategoryIds = null,
    string? Search = null,
    ShopSortOrder Sort = ShopSortOrder.Newest,
    int Page = 1,
    int PageSize = GetShopProductsQuery.DefaultPageSize) : IRequest<PagedResult<ProductCardDto>>
{
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 48;
    public const int MaxCategoryIds = 50;
}
