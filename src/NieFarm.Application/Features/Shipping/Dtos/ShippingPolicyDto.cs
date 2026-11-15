namespace NieFarm.Application.Features.Shipping.Dtos;

public record ShippingPolicyZoneDto(string Name, decimal Fee, IReadOnlyList<string> Provinces);

/// <summary>What the public pages quote: base fee, threshold, active zones and the fee range.</summary>
public record ShippingPolicyDto(
    decimal BaseFee,
    decimal? FreeShippingThreshold,
    IReadOnlyList<ShippingPolicyZoneDto> Zones,
    decimal MinFee,
    decimal MaxFee);
