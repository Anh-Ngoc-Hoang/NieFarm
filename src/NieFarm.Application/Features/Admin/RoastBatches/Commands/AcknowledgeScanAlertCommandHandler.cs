using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.RoastBatches.Commands;

/// <summary>
/// Loads the counter row tracked and saves it, writing only the acknowledgement column (R-26 D12): never an
/// UpdateAsync, which would mark every counter column modified and could overwrite a flush that just landed.
/// </summary>
public class AcknowledgeScanAlertCommandHandler(
    IRepositoryBase<RoastBatchScanStat> repository,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<AcknowledgeScanAlertCommand, Result>
{
    public async Task<Result> Handle(AcknowledgeScanAlertCommand request, CancellationToken cancellationToken)
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

        var stat = await repository.FirstOrDefaultAsync(new ScanStatByBatchIdSpec(request.BatchId), cancellationToken);
        if (stat is null || !stat.IsAlertActive)
            return Result.Success();

        stat.AcknowledgeAlert(DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
