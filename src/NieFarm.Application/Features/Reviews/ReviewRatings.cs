using Ardalis.Specification;
using NieFarm.Application.Features.Reviews.Dtos;
using NieFarm.Application.Features.Reviews.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Reviews;

/// <summary>
/// The one rating-average rule, shared by the PDP summary and the product cards so both always show
/// the same number. Only Approved reviews are ever passed in; this type knows nothing about who
/// bought what (R-17 adds a label, never changes the average).
/// </summary>
internal static class ReviewRatings
{
    /// <summary>Mean rounded to one decimal, midpoint away from zero; 0 for an empty set.</summary>
    internal static double Average(IEnumerable<int> ratings)
    {
        var count = 0;
        var sum = 0L;
        foreach (var rating in ratings)
        {
            count++;
            sum += rating;
        }

        return count == 0
            ? 0
            : Math.Round((double)sum / count, 1, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Approved-review summaries for many products in a single query. A product with no Approved
    /// review is absent from the result (the card then hides the rating).
    /// </summary>
    internal static async Task<IReadOnlyDictionary<int, ProductRatingSummary>> LoadSummariesAsync(
        IReadRepositoryBase<Review> reviews,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return new Dictionary<int, ProductRatingSummary>();

        var rows = await reviews.ListAsync(
            new ApprovedRatingsForProductsSpec(productIds.Distinct().ToList()), cancellationToken);

        return rows
            .GroupBy(r => r.ProductId)
            .ToDictionary(
                g => g.Key,
                g => new ProductRatingSummary(Average(g.Select(r => r.Rating)), g.Count()));
    }
}
