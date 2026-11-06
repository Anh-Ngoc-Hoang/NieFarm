namespace NieFarm.Application.Features.Orders;

/// <summary>Order code format (NF + 6 digits): generation and tolerant parsing of typed input.</summary>
public static class OrderCode
{
    public static string Generate() => $"NF{Random.Shared.Next(100_000, 1_000_000)}";

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
