namespace NieFarm.Application.Features.Shipping;

/// <summary>
/// Pure: maps a province name to the fee that applies. An unrecognised name, an unassigned province
/// or one in an inactive zone (the provider only returns active zones) pays the base fee.
/// </summary>
public static class ShippingZoneResolver
{
    public static ShippingQuote Resolve(ShippingRates rates, string? province)
    {
        var code = VietnamProvinceCatalog.TryResolve(province);
        if (code is null)
            return new ShippingQuote(rates.BaseFee, null, false);

        var zone = rates.Zones.FirstOrDefault(z => z.ProvinceCodes.Contains(code.Value));
        return zone is null
            ? new ShippingQuote(rates.BaseFee, null, true)
            : new ShippingQuote(zone.Fee, zone.Name, true);
    }
}
