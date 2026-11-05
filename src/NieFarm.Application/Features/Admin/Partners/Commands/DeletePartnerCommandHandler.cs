using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Partners.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Partners.Commands;

public class DeletePartnerCommandHandler(
    IRepositoryBase<Partner> repository,
    IImageStorage imageStorage)
    : IRequestHandler<DeletePartnerCommand, Result>
{
    public async Task<Result> Handle(DeletePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null)
            return Result.NotFound();

        var images = new[] { partner.LogoUrl }
            .Where(PartnerImageLimits.IsManagedImage)
            .ToList();

        await repository.DeleteAsync(partner, cancellationToken);
        await imageStorage.DeleteManyAsync(images, cancellationToken);

        return Result.Success();
    }
}
