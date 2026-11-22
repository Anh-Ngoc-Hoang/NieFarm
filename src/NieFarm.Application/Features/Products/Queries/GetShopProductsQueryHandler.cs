using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Models;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products.Queries;

public class GetShopProductsQueryHandler(
    IReadRepositoryBase<Product> repository,
    IReadRepositoryBase<Review> reviews)
    : IRequestHandler<GetShopProductsQuery, PagedResult<ProductCardDto>>
{
    public async Task<PagedResult<ProductCardDto>> Handle(
        GetShopProductsQuery request, CancellationToken cancellationToken)
    {
        // Clamp here, not in a validator: a query returning a plain DTO with a validator would
        // make ValidationBehavior throw, and a bad URL parameter must never be a 500.
        var search = ProductSearchTerm.Normalize(request.Search);
        var pageSize = Math.Clamp(request.PageSize, 1, GetShopProductsQuery.MaxPageSize);
        var categoryIds = request.CategoryIds is { Count: > 0 }
            ? request.CategoryIds.Distinct().Take(GetShopProductsQuery.MaxCategoryIds).ToList()
            : null;

        var total = await repository.CountAsync(
            new VisibleProductsSpec(categoryIds, search, request.Sort), cancellationToken);

        if (total == 0)
            return new PagedResult<ProductCardDto>([], 1, pageSize, 0);

        // Past the end falls back to the last page. Page comes from a URL, so clamp before the
        // multiplication below.
        var lastPage = (total + pageSize - 1) / pageSize;
        var page = Math.Clamp(request.Page, 1, lastPage);

        var products = await repository.ListAsync(
            new VisibleProductsSpec(categoryIds, search, request.Sort, (page - 1) * pageSize, pageSize),
            cancellationToken);

        var cards = await ProductCardAssembler.ToCardsAsync(products, reviews, cancellationToken);

        return new PagedResult<ProductCardDto>(cards, page, pageSize, total);
    }
}
