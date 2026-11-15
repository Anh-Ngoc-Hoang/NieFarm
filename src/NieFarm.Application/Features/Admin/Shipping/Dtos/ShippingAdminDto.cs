namespace NieFarm.Application.Features.Admin.Shipping.Dtos;

public record ShippingZoneAdminDto(
    int Id,
    string Name,
    decimal Fee,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<int> ProvinceCodes,
    IReadOnlyList<string> ProvinceNames);

/// <summary>One catalog province and the zone (if any) that currently holds it.</summary>
public record ShippingProvinceAdminDto(int Code, string Name, int? ZoneId, string? ZoneName);

public record ShippingAdminDto(
    decimal BaseFee,
    decimal? FreeShippingThreshold,
    bool SettingsMissing,
    IReadOnlyList<ShippingZoneAdminDto> Zones,
    IReadOnlyList<ShippingProvinceAdminDto> Provinces);
