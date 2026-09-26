using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Reviews.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Reviews.Commands;

public class SubmitProductReviewCommandHandler(
    IRepositoryBase<Review> repository,
    IReadRepositoryBase<Product> productReadRepository,
    ICurrentUser currentUser)
    : IRequestHandler<SubmitProductReviewCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SubmitProductReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result<int>.Unauthorized();

        var displayName = await currentUser.GetDisplayNameAsync(cancellationToken);
        var authorName = string.IsNullOrWhiteSpace(displayName) ? "Khách hàng" : displayName;

        // Discarded after the existence/visibility check — the one mixed read/write pattern
        // data-access.md explicitly permits (load via the read repo for a value check, then use
        // only the id, never the tracked entity, for the write).
        var product = await productReadRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsVisible)
            return Result<int>.NotFound();

        var pending = await repository.FirstOrDefaultAsync(
            new PendingReviewByAuthorSpec(request.ProductId, userId), cancellationToken);
        if (pending is not null)
            return Result<int>.Error("Bạn đang có một đánh giá chờ duyệt cho sản phẩm này.");

        var review = Review.Create(request.ProductId, userId, authorName, request.Rating, request.Comment);
        await repository.AddAsync(review, cancellationToken);

        return Result<int>.Success(review.Id);
    }
}
