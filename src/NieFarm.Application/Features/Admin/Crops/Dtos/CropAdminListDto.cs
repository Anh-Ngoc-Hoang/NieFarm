namespace NieFarm.Application.Features.Admin.Crops.Dtos;

public record CropAdminListDto(
    int Id,
    int Year,
    string SeasonLabel,
    string Headline,
    string ImageUrl,
    bool IsCurrent,
    int ProductCount);
