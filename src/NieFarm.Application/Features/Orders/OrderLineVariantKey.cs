using NieFarm.Application.Features.Carts;
using NieFarm.Application.Features.Products;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders;

/// <summary>A variant selection canonicalised against the current catalog.</summary>
internal readonly record struct ResolvedVariantKey(string Key, IReadOnlyList<string> Labels);

/// <summary>
/// Turns an old <see cref="OrderItem"/> back into a cart <c>VariantKey</c> for "buy again". The stored
/// <see cref="OrderItem.VariantKey"/> (or, for an order placed before that column existed, the display
/// <see cref="OrderItem.Variant"/> split on " / ") is only a hint: it is always re-canonicalised against the
/// product's CURRENT options through <see cref="ProductVariantResolver.Canonicalise"/>, so a renamed or removed
/// label fails to resolve and the cart can never receive a key the catalog does not know.
/// </summary>
internal static class OrderLineVariantKey
{
    private const string DisplaySeparator = " / ";

    /// <summary>
    /// The canonical key and labels, or null when the line's labels no longer resolve. A product with no options
    /// always resolves to the empty key. Needs <c>Options</c> (with values) loaded.
    /// </summary>
    internal static ResolvedVariantKey? Resolve(OrderItem item, Product product)
    {
        if (product.Options.Count == 0)
            return new ResolvedVariantKey(string.Empty, []);

        IReadOnlyList<string> submitted;
        if (!string.IsNullOrEmpty(item.VariantKey))
            submitted = CartLineKey.Split(item.VariantKey);
        else if (!string.IsNullOrWhiteSpace(item.Variant))
            submitted = item.Variant.Split(DisplaySeparator, StringSplitOptions.TrimEntries);
        else
            submitted = [];

        var labels = ProductVariantResolver.Canonicalise(product, submitted);
        return labels is null ? null : new ResolvedVariantKey(CartLineKey.From(labels), labels);
    }
}
