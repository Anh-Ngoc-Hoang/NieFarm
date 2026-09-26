using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Reviews.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Reviews.Commands;

/// <summary>No product repository: ProductId is immutable and was validated for existence/visibility at submit time.</summary>
public class EditProductReviewCommandHandler(
    IRepositoryBase<Review> repository,
    ICurrentUser currentUser)
    : IRequestHandler<EditProductReviewCommand, Result>
{
    public async Task<Result> Handle(EditProductReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();

        // Write repository for both the load and the save, per data-access.md — an entity
        // fetched from IReadRepositoryBase<T> lives on a different AppDbContext and would throw
        // a tracking conflict on UpdateAsync.
        var review = await repository.FirstOrDefaultAsync(
            new MyReviewByIdSpec(request.ReviewId, userId), cancellationToken);
        if (review is null)
            return Result.NotFound();   // also covers "not yours"

        if (!review.IsEditableByAuthor)
            return Result.Error("Đánh giá này đã được kiểm duyệt nên không thể chỉnh sửa.");

        review.Edit(request.Rating, request.Comment);
        await repository.UpdateAsync(review, cancellationToken);

        return Result.Success();
    }
}
