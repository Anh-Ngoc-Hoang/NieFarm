namespace NieFarm.Application.Features.Carts;

/// <summary>
/// Builds and reads the canonical <c>CartItem.VariantKey</c>: the selected option value labels,
/// in the product's own option SortOrder, joined with '|'. Empty for a product with no options.
///
/// <see cref="From"/> must only ever be given labels that came out of a <see cref="ProductVariantResolver"/>
/// canonicalisation against the product's own option values — never raw request strings — so the
/// stored key always agrees with the aggregate's ordinal comparison and the case-insensitive SQL
/// unique index on (CartId, ProductId, VariantKey).
/// </summary>
internal static class CartLineKey
{
    internal const char Separator = '|';
    internal const int MaxLength = 400;

    /// <summary>Canonical VariantKey: trimmed labels in option order, joined with '|'. Empty for no options.</summary>
    internal static string From(IReadOnlyList<string> canonicalLabels)
        => string.Join(Separator, canonicalLabels.Select(l => l.Trim().Replace(Separator, ' ')));

    /// <summary>Splits a stored key back into labels; empty key -> empty list.</summary>
    internal static IReadOnlyList<string> Split(string variantKey)
        => string.IsNullOrEmpty(variantKey)
            ? []
            : variantKey.Split(Separator);

    /// <summary>Display form for the cart line, e.g. "1kg / Xay pha phin". Empty for no options.</summary>
    internal static string ToDisplay(IReadOnlyList<string> labels)
        => string.Join(" / ", labels);
}
