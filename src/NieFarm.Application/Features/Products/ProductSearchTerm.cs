using System.Text;

namespace NieFarm.Application.Features.Products;

/// <summary>
/// The one place a shop search term is normalised, shared by the query handler and the
/// <c>/cua-hang</c> page so the URL, the input box and the SQL all agree.
/// </summary>
public static class ProductSearchTerm
{
    public const int MaxLength = 100;

    /// <summary>
    /// NFC-normalises (so a decomposed "e" + combining accent matches a precomposed "é"), trims,
    /// collapses runs of whitespace to one space and caps at <see cref="MaxLength"/> characters.
    /// Returns <c>null</c> when nothing is left, meaning "no search filter".
    /// </summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        string composed;
        try
        {
            composed = raw.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            // Invalid Unicode (a lone surrogate) cannot be normalised; treat it as no usable term.
            return null;
        }

        var collapsed = string.Join(' ', composed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (collapsed.Length == 0)
            return null;

        if (collapsed.Length > MaxLength)
        {
            // Do not cut a surrogate pair in half.
            var cut = MaxLength;
            if (char.IsHighSurrogate(collapsed[cut - 1]))
                cut--;
            collapsed = collapsed[..cut].TrimEnd();
        }

        return collapsed.Length == 0 ? null : collapsed;
    }
}
