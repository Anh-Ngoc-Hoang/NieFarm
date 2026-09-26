using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Activities.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Activities.Commands;

public class DeleteActivityCommandHandler(
    IRepositoryBase<Activity> repository,
    IImageStorage imageStorage)
    : IRequestHandler<DeleteActivityCommand, Result>
{
    public async Task<Result> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (activity is null)
            return Result.NotFound();

        var images = new[] { activity.ImageUrl }
            .Where(ActivityImageLimits.IsManagedImage)
            .ToList();

        await repository.DeleteAsync(activity, cancellationToken);
        await imageStorage.DeleteManyAsync(images, cancellationToken);

        return Result.Success();
    }
}
