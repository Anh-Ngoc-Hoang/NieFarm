using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Reviews.Commands;

public class DeleteReviewCommandHandler(IRepositoryBase<Review> repository)
    : IRequestHandler<DeleteReviewCommand, Result>
{
    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (review is null)
            return Result.NotFound();

        await repository.DeleteAsync(review, cancellationToken);

        return Result.Success();
    }
}
