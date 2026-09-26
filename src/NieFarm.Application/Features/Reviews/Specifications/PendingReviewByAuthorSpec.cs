using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>Backs the D5 guard: at most one pending review per (author, product).</summary>
public sealed class PendingReviewByAuthorSpec : Specification<Review>, ISingleResultSpecification<Review>
{
    public PendingReviewByAuthorSpec(int productId, string authorUserId)
    {
        Query.Where(r => r.ProductId == productId
                && r.AuthorUserId == authorUserId
                && r.Status == ReviewStatus.Pending);
    }
}
