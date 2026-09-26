using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Infrastructure.Services;

/// <summary>
/// Folds Vietnamese diacritics to ASCII and reduces the result to a URL-safe slug,
/// so "Cà phê Drip Bag (Hộp 10 gói)" becomes "ca-phe-drip-bag-hop-10-goi".
/// </summary>
public partial class SlugGenerator : ISlugGenerator
{
    public string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Đ/đ have no decomposed form, so they are replaced before normalising.
        var prepared = text.Replace('Đ', 'D').Replace('đ', 'd');

        var normalized = prepared.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                builder.Append(ch);
        }

        var ascii = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        ascii = NonSlugCharacters().Replace(ascii, "-");
        ascii = RepeatedSeparators().Replace(ascii, "-");

        return ascii.Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex("-{2,}")]
    private static partial Regex RepeatedSeparators();
}
