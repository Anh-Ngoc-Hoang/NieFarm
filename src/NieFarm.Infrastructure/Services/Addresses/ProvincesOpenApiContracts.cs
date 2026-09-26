namespace NieFarm.Infrastructure.Services.Addresses;

internal sealed record ProvinceResponse(string? Name, int Code);

internal sealed record ProvinceWithWardsResponse(string? Name, int Code, List<WardResponse>? Wards);

internal sealed record WardResponse(string? Name, int Code);
