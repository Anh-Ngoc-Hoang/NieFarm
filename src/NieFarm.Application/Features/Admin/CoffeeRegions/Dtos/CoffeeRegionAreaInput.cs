namespace NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;

/// <summary>
/// One area row as the admin form sends it. A null <see cref="Id"/> adds a new area; an existing
/// Id updates that area in place (CoffeeRegion.SetAreas). The list order is the display order.
/// </summary>
public record CoffeeRegionAreaInput(int? Id, string Name, string? Note);
