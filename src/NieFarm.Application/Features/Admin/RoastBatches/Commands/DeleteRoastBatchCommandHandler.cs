using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.RoastBatches.Commands;

public class DeleteRoastBatchCommandHandler(
    IRepositoryBase<RoastBatch> repository,
    IWriteContextRefresher contextRefresher,
    IBatchTraceCache cache,
    IImageStorage imageStorage)
    : IRequestHandler<DeleteRoastBatchCommand, Result>
{
    public async Task<Result> Handle(DeleteRoastBatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            contextRefresher.DiscardTrackedEntities();
        }
        catch (InvalidOperationException)
        {
            // Another operation left unsaved changes on this circuit's write context.
            return Result.Error("Could not load the batch. Reload the page and try again.");
        }

        var batch = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (batch is null)
            return Result.NotFound();

        if (batch.IsPublished)
        {
            return Result.Error(
                "Unpublish the batch before deleting it. Labels already printed for a published batch " +
                "would stop working.");
        }

        var images = new[] { batch.FarmImageUrl, batch.LabReportImageUrl }
            .Where(RoastBatchImageLimits.IsManagedImage)
            .ToList();

        // The database cascade removes the child rows and the scan counters.
        await repository.DeleteAsync(batch, cancellationToken);
        cache.Invalidate(batch.Code);

        if (images.Count > 0)
            await imageStorage.DeleteManyAsync(images, cancellationToken);

        return Result.Success();
    }
}
