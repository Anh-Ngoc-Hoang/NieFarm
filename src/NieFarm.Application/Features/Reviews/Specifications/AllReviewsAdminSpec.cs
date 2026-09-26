using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>The admin moderation list: optionally filtered by status, newest first, with the product name.</summary>
public sealed class AllReviewsAdminSpec : Specification<Review>
{
    public AllReviewsAdminSpec(ReviewStatus? status = null)
    {
        if (status is { } s)
            Query.Where(r => r.Status == s);

        Query.Include(r => r.Product)
             .OrderByDescending(r => r.CreatedAt);
    }
}
