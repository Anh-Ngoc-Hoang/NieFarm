using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Crops.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Crops.Commands;

/// <summary>
/// Exactly D1's two-step sequence: clear every current row other than the target in its own
/// SaveChanges, then mark the target in a second one. Never a single SaveChanges promoting one
/// row while demoting another — that is a value swap on a unique index, and statement order
/// inside one batch is not something to rely on.
/// </summary>
public class SetCurrentCropCommandHandler(IRepositoryBase<Crop> repository)
    : IRequestHandler<SetCurrentCropCommand, Result>
{
    public async Task<Result> Handle(SetCurrentCropCommand request, CancellationToken cancellationToken)
    {
        var currentRows = await repository.ListAsync(new CurrentCropsSpec(), cancellationToken);
        var toClear = currentRows.Where(c => c.Id != request.Id).ToList();

        if (toClear.Count > 0)
        {
            foreach (var crop in toClear)
                crop.ClearCurrent();

            await repository.UpdateRangeAsync(toClear, cancellationToken);
        }

        if (request.Id is not { } id)
            return Result.Success();

        var target = await repository.GetByIdAsync(id, cancellationToken);
        if (target is null)
            return Result.NotFound();

        // No-op success when the target is already the sole current crop.
        if (!target.IsCurrent)
        {
            target.MarkCurrent();
            await repository.UpdateAsync(target, cancellationToken);
        }

        return Result.Success();
    }
}
