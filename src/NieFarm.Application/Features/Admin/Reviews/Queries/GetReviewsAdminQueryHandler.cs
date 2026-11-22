using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Reviews.Dtos;
using NieFarm.Application.Features.Reviews;
using NieFarm.Application.Features.Reviews.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Reviews.Queries;

public class GetReviewsAdminQueryHandler(
    IReadRepositoryBase<Review> repository,
    IReadRepositoryBase<Order> orders)
    : IRequestHandler<GetReviewsAdminQuery, List<ReviewAdminListDto>>
{
    public async Task<List<ReviewAdminListDto>> Handle(GetReviewsAdminQuery request, CancellationToken cancellationToken)
    {
        var reviews = await repository.ListAsync(new AllReviewsAdminSpec(request.Status), cancellationToken);

        // R-17: one purchases query for the whole list (the admin page is behind the Admin role).
        var verified = await VerifiedPurchases.LoadAsync(
            orders,
            reviews.Select(r => (r.AuthorUserId, r.ProductId)).Distinct().ToList(),
            cancellationToken);

        return reviews.Select(r => new ReviewAdminListDto(
            r.Id,
            r.ProductId,
            r.Product?.Name ?? "—",
            r.AuthorName,
            r.AuthorUserId,
            r.Rating,
            r.Comment,
            r.Status,
            r.CreatedAt,
            r.ModeratedAt,
            r.CommentEditedAt,
            verified.Contains((r.AuthorUserId, r.ProductId)))).ToList();
    }
}
