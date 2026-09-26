using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Products.Dtos;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Products.Queries;

public class GetProductsAdminQueryHandler(IReadRepositoryBase<Product> repository)
    : IRequestHandler<GetProductsAdminQuery, List<ProductAdminListDto>>
{
    public async Task<List<ProductAdminListDto>> Handle(
        GetProductsAdminQuery request, CancellationToken cancellationToken)
    {
        var products = await repository.ListAsync(new AllProductsAdminSpec(), cancellationToken);

        return products.Select(p =>
        {
            var (minPrice, maxPrice) = ResolvePriceRange(p);

            return new ProductAdminListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Category?.Name ?? "—",
                p.Category?.Slug ?? string.Empty,
                p.Price,
                p.ImageUrl,
                p.Badge,
                p.IsFeatured,
                p.IsVisible,
                p.SortOrder,
                minPrice,
                maxPrice,
                p.Variants.Count);
        }).ToList();
    }

    /// <summary>
    /// The span of prices a product actually sells at. Computed over available variants, falling
    /// back to all of them when everything is sold out so the row still shows a number.
    /// </summary>
    private static (decimal? Min, decimal? Max) ResolvePriceRange(Product product)
    {
        if (product.Variants.Count == 0)
            return (null, null);

        var prices = product.Variants.Where(v => v.IsAvailable).Select(v => v.Price).ToList();
        if (prices.Count == 0)
            prices = product.Variants.Select(v => v.Price).ToList();

        return (prices.Min(), prices.Max());
    }
}
