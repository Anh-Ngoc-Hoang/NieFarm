using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>
/// One product's Approved reviews. Currently unused: the PDP derives its average from the visible
/// list (<c>GetProductReviewsQueryHandler</c>) and the shop cards use
/// <see cref="ApprovedRatingsForProductsSpec"/>. Kept for when the PDP list gets paged and the
/// aggregate must move to its own query (average through <c>ReviewRatings.Average</c>).
/// </summary>
public sealed class ApprovedReviewsForProductSpec : Specification<Review>
{
    public ApprovedReviewsForProductSpec(int productId)
    {
        Query.Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved);
    }
}
