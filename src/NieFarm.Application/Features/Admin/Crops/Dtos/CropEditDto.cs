namespace NieFarm.Application.Features.Admin.Crops.Dtos;

public record CropEditDto(
    int Id,
    int Year,
    string SeasonLabel,
    string Headline,
    string Story,
    string Region,
    string Altitude,
    string Varieties,
    string Process,
    string TastingNotes,
    string ImageUrl,
    string ImageAlt,
    IReadOnlyList<int> ProductIds,
    bool IsCurrent);
