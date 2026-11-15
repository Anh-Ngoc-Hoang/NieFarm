namespace NieFarm.Application.Features.Orders;

/// <summary>Order code format (NF + 6 digits): generation and tolerant parsing of typed input.</summary>
public static class OrderCode
{
    public static string Generate() => $"NF{Random.Shared.Next(100_000, 1_000_000)}";

    private static readonly System.Text.RegularExpressions.Regex Token = new(
        "(?<![A-Za-z0-9])NF[0-9]{6}(?![A-Za-z0-9])",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Finds an order code inside free text such as a bank transfer description: NF plus 6 digits,
    /// case-insensitive, with token boundaries. Searches <paramref name="content"/> first and
    /// <paramref name="code"/> second. Returns true only when exactly one distinct code is found;
    /// <paramref name="ambiguous"/> is true when more than one distinct code is found.
    /// </summary>
    public static bool TryFind(string? content, string? code, out string found, out bool ambiguous)
    {
        found = "";
        ambiguous = false;

        foreach (var text in new[] { content, code })
        {
            if (string.IsNullOrWhiteSpace(text)) continue;

            var distinct = Token.Matches(text)
                .Select(m => m.Value.ToUpperInvariant())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (distinct.Count == 0) continue;
            if (distinct.Count > 1)
            {
                ambiguous = true;
                return false;
            }

            found = distinct[0];
            return true;
        }

        return false;
    }

    /// <summary>Trims, strips whitespace, upper-cases; six bare digits get the NF prefix.</summary>
    public static bool TryNormalise(string? raw, out string code)
    {
        code = "";
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var s = new string(raw.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (s.Length == 6 && s.All(char.IsAsciiDigit))
            s = "NF" + s;

        if (s.Length != 8 || !s.StartsWith("NF", StringComparison.Ordinal) || !s.Skip(2).All(char.IsAsciiDigit))
            return false;

        code = s;
        return true;
    }
}
