using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;

public record CoffeeRegionAdminListDto(
    CoffeeRegionKey Key,
    string Title,
    string Tagline,
    string ImageUrl,
    int AreaCount,
    DateTime UpdatedAt);
