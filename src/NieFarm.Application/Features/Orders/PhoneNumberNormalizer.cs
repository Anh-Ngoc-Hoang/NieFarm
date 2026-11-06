namespace NieFarm.Application.Features.Orders;

/// <summary>Reduces a phone number to digits in canonical national form so differently formatted inputs compare equal.</summary>
public static class PhoneNumberNormalizer
{
    public static string Normalise(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";

        var digits = new string(raw.Where(char.IsAsciiDigit).ToArray());

        if (digits.StartsWith("0084", StringComparison.Ordinal))
            digits = digits[2..];

        if (digits.StartsWith("84", StringComparison.Ordinal) && digits.Length >= 11)
        {
            var rest = digits[2..];
            return rest.StartsWith('0') ? rest : "0" + rest;
        }

        if (digits.Length == 9 && !digits.StartsWith('0'))
            return "0" + digits;

        return digits;
    }

    public static bool Matches(string stored, string typed)
    {
        var a = Normalise(stored);
        var b = Normalise(typed);
        return a.Length > 0 && b.Length > 0 && string.Equals(a, b, StringComparison.Ordinal);
    }
}
