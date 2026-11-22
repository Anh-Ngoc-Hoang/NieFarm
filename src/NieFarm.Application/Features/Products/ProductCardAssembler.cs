using Ardalis.Specification;
using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Reviews;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products;

/// <summary>
/// Builds the cards for a list of products with their Approved-review rating, using one extra
/// query for the whole list (no N+1). Used by the shop, home featured and PDP related handlers.
/// The products must be loaded the way <see cref="ProductCardMapper"/> requires.
/// </summary>
internal static class ProductCardAssembler
{
    internal static async Task<IReadOnlyList<ProductCardDto>> ToCardsAsync(
        IReadOnlyList<Product> products,
        IReadRepositoryBase<Review> reviews,
        CancellationToken cancellationToken)
    {
        if (products.Count == 0)
            return [];

        var summaries = await ReviewRatings.LoadSummariesAsync(
            reviews, products.Select(p => p.Id).ToList(), cancellationToken);

        return products
            .Select(p => ProductCardMapper.ToCard(p, summaries.GetValueOrDefault(p.Id)))
            .ToList();
    }
}
