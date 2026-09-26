namespace NieFarm.Application.Features.Admin.Activities.Dtos;

public record ActivityAdminListDto(
    int Id,
    string Title,
    string Slug,
    DateOnly EventDate,
    string ImageUrl,
    bool IsPublished);
