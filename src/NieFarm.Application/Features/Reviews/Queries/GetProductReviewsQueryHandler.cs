using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Reviews.Dtos;
using NieFarm.Application.Features.Reviews.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Reviews.Queries;

public class GetProductReviewsQueryHandler(
    IReadRepositoryBase<Review> repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetProductReviewsQuery, ProductReviewsDto>
{
    public async Task<ProductReviewsDto> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        // One round trip; splitting the approved subset from this same in-memory list is safe
        // because the page renders the whole thing. If paging is added later, the
        // average/count must move to a separate CountAsync/aggregate query over
        // ApprovedReviewsForProductSpec instead of being derived from the page's own list.
        var reviews = await repository.ListAsync(
            new ProductReviewsVisibleToSpec(request.ProductId, userId), cancellationToken);

        var approved = reviews.Where(r => r.Status == ReviewStatus.Approved).ToList();
        var averageRating = approved.Count == 0
            ? 0
            : Math.Round(approved.Average(r => (double)r.Rating), 1, MidpointRounding.AwayFromZero);

        // Display order: the caller's own pending review (at most one, per D5) first, then
        // approved reviews newest-first.
        var ordered = reviews
            .OrderByDescending(r => r.Status == ReviewStatus.Pending)
            .ThenByDescending(r => r.CreatedAt)
            .Select(r => new ProductReviewDto(
                r.Id, r.AuthorName, r.Rating, r.Comment, r.CreatedAt, r.Status == ReviewStatus.Pending))
            .ToList();

        return new ProductReviewsDto(averageRating, approved.Count, ordered);
    }
}
