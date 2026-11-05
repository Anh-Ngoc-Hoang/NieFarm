using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;

/// <summary>Areas are in display order.</summary>
public record CoffeeRegionEditDto(
    CoffeeRegionKey Key,
    string Title,
    string Tagline,
    string Description,
    string Altitude,
    string Varieties,
    string Climate,
    string FlavorProfile,
    string ImageUrl,
    string ImageAlt,
    IReadOnlyList<CoffeeRegionAreaInput> Areas);
