namespace NieFarm.Application.Features.Admin.Partners.Dtos;

public record PartnerAdminListDto(
    int Id,
    string Name,
    string LogoUrl,
    string WebsiteUrl,
    int SortOrder,
    bool IsVisible);
