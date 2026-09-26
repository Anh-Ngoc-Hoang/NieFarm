using MediatR;
using NieFarm.Application.Features.Admin.Reviews.Dtos;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Reviews.Queries;

public record GetReviewsAdminQuery(ReviewStatus? Status = null) : IRequest<List<ReviewAdminListDto>>;
