namespace NieFarm.Application.Features.Shipping;

/// <summary>An active zone as pricing sees it: a fee and the catalog province codes it covers.</summary>
public record ShippingZoneRate(int Id, string Name, decimal Fee, IReadOnlySet<int> ProvinceCodes);

/// <summary>The shipping configuration in force: base fee, optional nationwide threshold, active zones.</summary>
public record ShippingRates(
    decimal BaseFee,
    decimal? FreeShippingThreshold,
    IReadOnlyList<ShippingZoneRate> Zones)
{
    /// <summary>What the provider returns when the settings row is missing or invalid.</summary>
    public const decimal FallbackBaseFee = 30_000m;

    public static ShippingRates Fallback { get; } = new(FallbackBaseFee, null, []);
}

/// <summary>The fee that applies to a province: the zone fee or the base fee.</summary>
public record ShippingQuote(decimal Fee, string? ZoneName, bool ProvinceResolved);
