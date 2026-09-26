using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>
/// The single authority on storefront review visibility: every approved review on the product,
/// plus — only when <paramref name="currentUserId"/> is non-null — that caller's own still-pending
/// review. Anonymous callers (null id) see approved rows only. Rejected reviews are never
/// returned here, not even to their own author — see docs/plans/product-reviews.md.
/// </summary>
public sealed class ProductReviewsVisibleToSpec : Specification<Review>
{
    public ProductReviewsVisibleToSpec(int productId, string? currentUserId)
    {
        Query.Where(r => r.ProductId == productId
                && (r.Status == ReviewStatus.Approved
                    || (currentUserId != null && r.AuthorUserId == currentUserId && r.Status == ReviewStatus.Pending)))
             .OrderByDescending(r => r.CreatedAt);
    }
}
