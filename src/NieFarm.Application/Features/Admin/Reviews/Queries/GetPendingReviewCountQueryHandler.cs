using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Reviews.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Reviews.Queries;

public class GetPendingReviewCountQueryHandler(IReadRepositoryBase<Review> repository)
    : IRequestHandler<GetPendingReviewCountQuery, int>
{
    public Task<int> Handle(GetPendingReviewCountQuery request, CancellationToken cancellationToken) =>
        repository.CountAsync(new PendingReviewsSpec(), cancellationToken);
}
