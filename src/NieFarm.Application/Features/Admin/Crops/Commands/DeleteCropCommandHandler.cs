using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Crops.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Crops.Commands;

public class DeleteCropCommandHandler(
    IRepositoryBase<Crop> repository,
    IImageStorage imageStorage)
    : IRequestHandler<DeleteCropCommand, Result>
{
    public async Task<Result> Handle(DeleteCropCommand request, CancellationToken cancellationToken)
    {
        var crop = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (crop is null)
            return Result.NotFound();

        var images = new[] { crop.ImageUrl }
            .Where(CropImageLimits.IsManagedImage)
            .ToList();

        // The database cascade (CropConfiguration) removes the CropProducts links.
        await repository.DeleteAsync(crop, cancellationToken);
        await imageStorage.DeleteManyAsync(images, cancellationToken);

        return Result.Success();
    }
}
