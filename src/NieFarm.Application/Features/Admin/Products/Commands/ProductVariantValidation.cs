using NieFarm.Application.Features.Admin.Products.Dtos;

namespace NieFarm.Application.Features.Admin.Products.Commands;

/// <summary>
/// Rules for the option/variant matrix, shared by the command validator and kept separate from
/// it so the whole matrix can be checked as one unit rather than property by property.
/// Messages are English — this only ever surfaces in the admin UI (see .claude/rules/ui.md).
/// </summary>
public static class ProductVariantValidation
{
    public static IEnumerable<string> Validate(
        List<ProductOptionInput> options, List<ProductVariantInput> variants)
    {
        options ??= [];
        variants ??= [];

        if (options.Count > ProductVariantLimits.MaxOptions)
        {
            yield return $"A product can have at most {ProductVariantLimits.MaxOptions} options.";
            yield break;
        }

        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var option in options)
        {
            var name = option.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
                yield return "Every option needs a name.";
            else if (name.Length > ProductVariantLimits.MaxOptionNameLength)
                yield return $"Option name \"{name}\" is longer than {ProductVariantLimits.MaxOptionNameLength} characters.";
            else if (!seenNames.Add(name))
                yield return $"Option \"{name}\" is listed more than once.";

            var values = (option.Values ?? []).Select(v => v?.Trim() ?? string.Empty)
                .Where(v => v.Length > 0).ToList();

            if (values.Count == 0)
                yield return $"Option \"{name}\" needs at least one value.";
            else if (values.Count > ProductVariantLimits.MaxValuesPerOption)
                yield return $"Option \"{name}\" has more than {ProductVariantLimits.MaxValuesPerOption} values.";

            if (values.Any(v => v.Length > ProductVariantLimits.MaxValueLabelLength))
                yield return $"A value of option \"{name}\" is longer than {ProductVariantLimits.MaxValueLabelLength} characters.";

            if (values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != values.Count)
                yield return $"Option \"{name}\" has duplicate values.";
        }

        // Anything below compares variants against the option shape, so a broken shape would
        // only produce noise on top of the messages already reported.
        var shapedOptions = options
            .Select(o => (o.Values ?? []).Select(v => v?.Trim() ?? string.Empty)
                .Where(v => v.Length > 0).ToList())
            .ToList();

        if (options.Count == 0)
        {
            if (variants.Count > 0)
                yield return "Remove the variants or add at least one option.";
            yield break;
        }

        if (shapedOptions.Any(v => v.Count == 0))
            yield break;

        var expected = shapedOptions.Aggregate(1, (acc, v) => acc * v.Count);

        if (expected > ProductVariantLimits.MaxVariants)
        {
            yield return $"That is {expected} combinations — the maximum is {ProductVariantLimits.MaxVariants}. Reduce the number of option values.";
            yield break;
        }

        if (variants.Count != expected)
        {
            yield return "Every option combination must have a variant row.";
            yield break;
        }

        var seenCombinations = new HashSet<string>();
        foreach (var variant in variants)
        {
            var indexes = variant.ValueIndexes ?? [];

            if (indexes.Count != options.Count)
            {
                yield return "Every variant must select exactly one value per option.";
                yield break;
            }

            for (var i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] < 0 || indexes[i] >= shapedOptions[i].Count)
                {
                    yield return "A variant refers to an option value that does not exist.";
                    yield break;
                }
            }

            if (!seenCombinations.Add(string.Join('|', indexes)))
            {
                yield return "Two variants use the same option combination.";
                yield break;
            }

            if (variant.Price < 0)
                yield return "Variant price cannot be negative.";

            if (variant.ImageUrl is { Length: > 500 })
                yield return "A variant image URL is too long.";
        }
    }
}
