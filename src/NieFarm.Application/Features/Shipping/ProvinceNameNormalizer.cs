using System.Globalization;
using System.Text;

namespace NieFarm.Application.Features.Shipping;

/// <summary>
/// Reduces a province name to a comparison key: decomposed, diacritics dropped, đ to d, lower
/// case, punctuation to spaces, whitespace collapsed, and a leading administrative prefix
/// (tinh, thanh pho, tp) removed. Matching against the key is exact; there is no fuzzy matching
/// (it could put an order in the wrong zone, e.g. "Hà Nam" vs "Hà Nội").
/// </summary>
public static class ProvinceNameNormalizer
{
    private static readonly string[] Prefixes = ["thanh pho ", "tinh ", "tp "];

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        var lastWasSpace = true;

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            var c = ch switch { 'đ' or 'Đ' => 'd', _ => char.ToLowerInvariant(ch) };
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                lastWasSpace = false;
            }
            else if (!lastWasSpace)
            {
                sb.Append(' ');
                lastWasSpace = true;
            }
        }

        var key = sb.ToString().TrimEnd();
        foreach (var prefix in Prefixes)
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal) && key.Length > prefix.Length)
            {
                key = key[prefix.Length..];
                break;
            }
        }

        return key;
    }
}
