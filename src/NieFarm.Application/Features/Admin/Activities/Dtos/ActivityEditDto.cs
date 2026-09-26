using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Activities.Dtos;

public record ActivityEditDto(
    int Id,
    string Title,
    string Slug,
    string ModalTitle,
    DateOnly EventDate,
    string ImageUrl,
    string ImageAlt,
    ActivityCardSize CardSize,
    string Body,
    bool IsPublished);
