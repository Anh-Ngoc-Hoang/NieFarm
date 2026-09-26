namespace NieFarm.Application.Features.Addresses.Dtos;

/// <summary>A ward/commune belonging to a province. There is no district level after 2025.</summary>
public record WardDto(int Code, string Name);
