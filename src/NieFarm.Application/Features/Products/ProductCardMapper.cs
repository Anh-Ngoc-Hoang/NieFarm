using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products;

/// <summary>
/// The one place a <see cref="ProductCardDto"/> is built. The product must be loaded with its
/// Category, Options and Variants (with values) so the card can tell whether there is anything to
/// choose and whether anything is purchasable.
/// </summary>
internal static class ProductCardMapper
{
    internal static ProductCardDto ToCard(Product p) => new(
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
        ProductAvailability.IsPurchasable(p));
}
