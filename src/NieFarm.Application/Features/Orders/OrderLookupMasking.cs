namespace NieFarm.Application.Features.Orders;

/// <summary>Masking for the code + phone order lookup, which is weaker proof than signing in.</summary>
public static class OrderLookupMasking
{
    public static string MaskName(string? name)
    {
        var words = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0) return "";
        if (words.Length == 1) return words[0][..1] + "***";

        var initials = words[..^1].Select(w => char.ToUpperInvariant(w[0]) + ".");
        return string.Join(" ", initials.Append(words[^1]));
    }

    public static string MaskPhone(string normalisedPhone)
    {
        var p = normalisedPhone ?? "";
        if (p.Length < 4) return new string('•', p.Length);
        return new string('•', p.Length - 3) + p[^3..];
    }

    /// <summary>Last two address segments (ward, province); never the street.</summary>
    public static string DeliveryArea(string? shippingAddress)
    {
        var parts = (shippingAddress ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return "";
        if (parts.Length <= 2) return parts[^1];
        return parts[^2] + ", " + parts[^1];
    }
}
