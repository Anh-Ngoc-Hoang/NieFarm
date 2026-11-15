using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products;

/// <summary>
/// Single authority on "can this product (or this resolved variant) be bought right now". Used by
/// the product card mapper, <c>CartAssembler</c> and <c>AddToCartCommandHandler</c>. A product with
/// no options has no variant to mark unavailable, so it is always purchasable; stock-level
/// availability (R-06) plugs in here.
/// </summary>
internal static class ProductAvailability
{
    /// <summary>
    /// True when the product has no options, or at least one resolvable variant is available.
    /// Needs <c>Options</c>/<c>Variants</c> (with their values) loaded.
    /// </summary>
    internal static bool IsPurchasable(Product product)
        => product.Options.Count == 0
           || ProductVariantResolver.BuildVariants(product).Any(v => v.IsAvailable);

    /// <summary>
    /// Availability of one cart line / selection: no options means always available, otherwise the
    /// resolved variant must exist and be available.
    /// </summary>
    internal static bool IsVariantPurchasable(Product product, ProductVariantDto? variant)
        => product.Options.Count == 0 || variant is { IsAvailable: true };
}
