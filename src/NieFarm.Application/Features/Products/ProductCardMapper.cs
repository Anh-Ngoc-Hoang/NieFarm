using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Application.Features.Reviews.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products;

/// <summary>
/// The one place a <see cref="ProductCardDto"/> is built. The product must be loaded with its
/// Category, Options and Variants (with values) so the card can tell whether there is anything to
/// choose and whether anything is purchasable.
/// </summary>
internal static class ProductCardMapper
{
    /// <summary>A card with no rating (no review lookup was made, e.g. the crop's product list).</summary>
    internal static ProductCardDto ToCard(Product p) => ToCard(p, null);

    internal static ProductCardDto ToCard(Product p, ProductRatingSummary? rating) => new(
        p.Slug,
        p.Name,
        p.Category?.Name ?? string.Empty,
        p.Price,
        p.ImageUrl,
        p.ImageAlt,
        string.IsNullOrWhiteSpace(p.Badge) ? null : p.Badge,
        p.Summary,
        p.Id,
        p.Options.Count > 0,
        ProductAvailability.IsPurchasable(p),
        rating is { ReviewCount: > 0 } ? rating.AverageRating : null,
        rating?.ReviewCount ?? 0);
}
