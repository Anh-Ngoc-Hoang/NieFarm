using NieFarm.Application.Features.Products.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Products;

/// <summary>
/// Single authority on "which labels name which variant" for a loaded <see cref="Product"/>
/// (with its Options/Variants navigations included). Used by the product detail query, the cart
/// add/set commands and <c>CartAssembler</c> — one positional label-based model shared everywhere
/// a shopper's selection has to be resolved against the catalog.
/// </summary>
internal static class ProductVariantResolver
{
    internal static IReadOnlyList<ProductOption> OrderedOptions(Product product)
        => product.Options.OrderBy(o => o.SortOrder).ToList();

    internal static IReadOnlyList<ProductOptionDto> BuildOptions(Product product)
        => OrderedOptions(product)
            .Select(o => new ProductOptionDto(
                o.Name,
                o.Values.OrderBy(v => v.SortOrder).Select(v => v.Label).ToList()))
            .ToList();

    /// <summary>Positional label mapping; variants that do not name every axis are skipped.</summary>
    internal static IReadOnlyList<ProductVariantDto> BuildVariants(Product product)
    {
        var orderedOptions = OrderedOptions(product);

        // The variant labels are addressed positionally, so every stored option value id is
        // mapped back to its (option index, label) once, up front.
        var positionByValueId = new Dictionary<int, (int OptionIndex, string Label)>();
        for (var optionIndex = 0; optionIndex < orderedOptions.Count; optionIndex++)
        {
            foreach (var value in orderedOptions[optionIndex].Values.OrderBy(v => v.SortOrder))
                positionByValueId[value.Id] = (optionIndex, value.Label);
        }

        var variants = new List<ProductVariantDto>();
        foreach (var variant in product.Variants.OrderBy(v => v.SortOrder))
        {
            var labels = new string[orderedOptions.Count];
            var resolved = 0;

            foreach (var value in variant.Values)
            {
                if (!positionByValueId.TryGetValue(value.ProductOptionValueId, out var position))
                    continue;

                labels[position.OptionIndex] = position.Label;
                resolved++;
            }

            // A variant that does not name every axis cannot be shown; skip it rather than
            // render a broken selection.
            if (resolved != orderedOptions.Count)
                continue;

            variants.Add(new ProductVariantDto(labels, variant.Price, variant.ImageUrl, variant.IsAvailable));
        }

        return variants;
    }

    /// <summary>
    /// The variant whose value labels exactly match the given canonical labels (ordinal), if
    /// any. Shared by the cart write path (AddToCartCommand / SetCartLineQuantityCommand) and
    /// CartAssembler.
    /// </summary>
    internal static ProductVariantDto? FindVariant(Product product, IReadOnlyList<string> labels)
        => BuildVariants(product).FirstOrDefault(v => v.ValueLabels.SequenceEqual(labels, StringComparer.Ordinal));

    /// <summary>
    /// Canonicalises caller-supplied labels against the product's own option values
    /// (trim + OrdinalIgnoreCase match, positional). Returns null when the count is wrong or any
    /// label is not a real value of its axis. This is the security boundary: it is what turns
    /// "the browser sent 250g" into "the catalog's own 250g string, at position 0", and rejects
    /// anything else.
    /// </summary>
    internal static IReadOnlyList<string>? Canonicalise(Product product, IReadOnlyList<string> submittedLabels)
    {
        var orderedOptions = OrderedOptions(product);

        if (submittedLabels.Count != orderedOptions.Count)
            return null;

        var canonical = new string[orderedOptions.Count];
        for (var i = 0; i < orderedOptions.Count; i++)
        {
            var submitted = submittedLabels[i]?.Trim() ?? string.Empty;
            var match = orderedOptions[i].Values
                .FirstOrDefault(v => string.Equals(v.Label, submitted, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                return null;

            canonical[i] = match.Label;
        }

        return canonical;
    }
}
