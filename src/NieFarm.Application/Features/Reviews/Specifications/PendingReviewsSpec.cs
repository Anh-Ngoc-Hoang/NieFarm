using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>Backs the pending-reviews sidebar badge and the admin moderation queue.</summary>
public sealed class PendingReviewsSpec : Specification<Review>
{
    public PendingReviewsSpec()
    {
        Query.Where(r => r.Status == ReviewStatus.Pending)
             .OrderBy(r => r.CreatedAt);
    }
}
