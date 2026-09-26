using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

public class RejectReviewCommandHandler(
    IRepositoryBase<Review> repository,
    ICurrentUser currentUser)
    : IRequestHandler<RejectReviewCommand, Result>
{
    public async Task<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var moderatorId = await currentUser.GetUserIdAsync(cancellationToken);
        if (moderatorId is null)
            return Result.Unauthorized();

        var review = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (review is null)
            return Result.NotFound();

        review.Reject(moderatorId);
        await repository.UpdateAsync(review, cancellationToken);

        return Result.Success();
    }
}
