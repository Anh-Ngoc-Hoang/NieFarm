using Ardalis.Specification;
using NieFarm.Application.Features.Reviews.Dtos;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Specifications;

/// <summary>
/// (ProductId, Rating) of every Approved review for a set of products — one round trip for a whole
/// card grid. Pending and Rejected reviews never count. Folded into averages in memory by
/// <see cref="ReviewRatings"/>.
/// </summary>
public sealed class ApprovedRatingsForProductsSpec : Specification<Review, ProductRatingRow>
{
    public ApprovedRatingsForProductsSpec(IReadOnlyCollection<int> productIds)
    {
        Query.Where(r => productIds.Contains(r.ProductId) && r.Status == ReviewStatus.Approved)
            .AsNoTracking();

        Query.Select(r => new ProductRatingRow(r.ProductId, r.Rating));
    }
}
