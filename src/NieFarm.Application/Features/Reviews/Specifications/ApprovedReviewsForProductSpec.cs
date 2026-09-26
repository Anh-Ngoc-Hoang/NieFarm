using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>Used where only the approved aggregate (average/count) matters, not the list itself.</summary>
public sealed class ApprovedReviewsForProductSpec : Specification<Review>
{
    public ApprovedReviewsForProductSpec(int productId)
    {
        Query.Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved);
    }
}
