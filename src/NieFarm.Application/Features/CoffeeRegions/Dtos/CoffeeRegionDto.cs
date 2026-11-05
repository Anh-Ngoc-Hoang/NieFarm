using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.CoffeeRegions.Dtos;

/// <summary>
/// One region's story for the home page's "Vietnam Coffee Stories" panel. The fixed map label
/// ("Tây Nguyên – Nam Bộ") and the fact labels ("Độ cao", "Giống chủ đạo", …) are presentation and live in
/// Web, not here. <see cref="ImageUrl"/> is null when the region has no photo.
/// </summary>
public record CoffeeRegionDto(
    CoffeeRegionKey Key,
    string Title,
    string Tagline,
    IReadOnlyList<string> DescriptionParagraphs,
    string Altitude,
    string Varieties,
    string Climate,
    string FlavorProfile,
    string? ImageUrl,
    string ImageAlt,
    IReadOnlyList<CoffeeRegionAreaDto> Areas);

public record CoffeeRegionAreaDto(string Name, string Note);
