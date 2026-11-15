using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Shipping.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Shipping.Commands;

/// <summary>Deletes a zone; its provinces go back to the base fee. Past orders keep their own zone-name snapshot.</summary>
public record DeleteShippingZoneCommand(int Id) : IRequest<Result>;

public class DeleteShippingZoneCommandHandler(
    IRepositoryBase<ShippingZone> repository,
    IWriteContextRefresher contextRefresher)
    : IRequestHandler<DeleteShippingZoneCommand, Result>
{
    public async Task<Result> Handle(DeleteShippingZoneCommand request, CancellationToken cancellationToken)
    {
        contextRefresher.DiscardTrackedEntities();

        var zone = await repository.FirstOrDefaultAsync(new ShippingZoneByIdSpec(request.Id), cancellationToken);
        if (zone is null)
            return Result.NotFound();

        await repository.DeleteAsync(zone, cancellationToken);
        return Result.Success();
    }
}
