using MediatR;

namespace NieFarm.Application.Features.Admin.Reviews.Queries;

public record GetPendingReviewCountQuery : IRequest<int>;
