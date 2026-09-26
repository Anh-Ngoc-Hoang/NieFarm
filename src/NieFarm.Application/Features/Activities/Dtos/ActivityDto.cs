using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Activities.Dtos;

public record ActivityDto(
    int Id,
    string Slug,
    string Title,
    string ModalTitle,
    DateOnly EventDate,
    string ImageUrl,
    string ImageAlt,
    ActivityCardSize CardSize,
    string Body);
