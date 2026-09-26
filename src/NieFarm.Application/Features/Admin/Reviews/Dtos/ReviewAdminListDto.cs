using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Reviews.Dtos;

public record ReviewAdminListDto(
    int Id,
    int ProductId,
    string ProductName,
    string AuthorName,
    string AuthorUserId,
    int Rating,
    string Comment,
    ReviewStatus Status,
    DateTime CreatedAt,
    DateTime? ModeratedAt,
    DateTime? CommentEditedAt);
