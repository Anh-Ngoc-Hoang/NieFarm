using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

/// <summary>
/// D12: legal in any status. D13: leaves Status/ModeratedAt/ModeratedByUserId untouched. D6
/// residual risk applies — this relies on the page's [Authorize(Roles = "Admin,SuperAdmin")]
/// rather than an in-handler role check, consistent with the other three moderation handlers.
/// </summary>
public class EditReviewCommentCommandHandler(
    IRepositoryBase<Review> repository,
    ICurrentUser currentUser)
    : IRequestHandler<EditReviewCommentCommand, Result>
{
    public async Task<Result> Handle(EditReviewCommentCommand request, CancellationToken cancellationToken)
    {
        var editorId = await currentUser.GetUserIdAsync(cancellationToken);
        if (editorId is null)
            return Result.Unauthorized();

        var review = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (review is null)
            return Result.NotFound();

        review.EditComment(request.Comment, editorId);
        await repository.UpdateAsync(review, cancellationToken);

        return Result.Success();
    }
}
