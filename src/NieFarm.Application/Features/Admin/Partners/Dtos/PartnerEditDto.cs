namespace NieFarm.Application.Features.Admin.Partners.Dtos;

public record PartnerEditDto(
    int Id,
    string Name,
    string LogoUrl,
    string WebsiteUrl,
    int SortOrder,
    bool IsVisible);
