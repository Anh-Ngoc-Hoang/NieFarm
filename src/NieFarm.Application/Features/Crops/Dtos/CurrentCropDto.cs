using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Crops.Dtos;

/// <summary>
/// The current "New Crop" for the home page. Fact labels ("Vùng trồng", "Độ cao", "Giống",
/// "Sơ chế") are presentation and live in Razor, not here.
/// </summary>
public record CurrentCropDto(
    int Year,
    string SeasonLabel,
    string Headline,
    IReadOnlyList<string> StoryParagraphs,
    string Region,
    string Altitude,
    string Varieties,
    string Process,
    IReadOnlyList<string> TastingNotes,
    string ImageUrl,
    string ImageAlt,
    IReadOnlyList<ProductCardDto> Products);
