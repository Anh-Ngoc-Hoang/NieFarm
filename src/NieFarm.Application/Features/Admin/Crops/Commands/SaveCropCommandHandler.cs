using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Crops.Dtos;
using NieFarm.Application.Features.Crops.Specifications;
using NieFarm.Application.Features.Products.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Crops.Commands;

public class SaveCropCommandHandler(
    IRepositoryBase<Crop> repository,
    IReadRepositoryBase<Product> productReadRepository,
    IImageStorage imageStorage)
    : IRequestHandler<SaveCropCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveCropCommand request, CancellationToken cancellationToken)
    {
        var clash = await repository.FirstOrDefaultAsync(
            new CropByYearSpec(request.Year, request.Id), cancellationToken);
        if (clash is not null)
            return Result<int>.Error($"A crop for {request.Year} already exists.");

        var distinctIds = request.ProductIds.Distinct().ToList();
        if (distinctIds.Count > 0)
        {
            // Value check only, then discarded — never passed to the write repository
            // (data-access.md's "mixed read + write" rule).
            var existingProducts = await productReadRepository.ListAsync(
                new ProductsByIdsSpec(distinctIds, visibleOnly: false), cancellationToken);

            if (existingProducts.Count != distinctIds.Count)
                return Result<int>.Error("One or more selected products no longer exist.");
        }

        if (request.Id is { } id)
        {
            var existing = await repository.FirstOrDefaultAsync(
                new CropByIdWithProductsSpec(id), cancellationToken);
            if (existing is null)
                return Result<int>.NotFound();

            var previousImageUrl = existing.ImageUrl;

            existing.Update(
                request.Year, request.SeasonLabel, request.Headline, request.Story,
                request.Region, request.Altitude, request.Varieties, request.Process,
                request.TastingNotes, request.ImageUrl, request.ImageAlt);
            existing.SetProducts(request.ProductIds);

            await repository.UpdateAsync(existing, cancellationToken);

            if (CropImageLimits.IsManagedImage(previousImageUrl)
                && !string.Equals(previousImageUrl, existing.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                await imageStorage.DeleteManyAsync([previousImageUrl], cancellationToken);
            }

            return Result<int>.Success(existing.Id);
        }

        var crop = Crop.Create(
            request.Year, request.SeasonLabel, request.Headline, request.Story,
            request.Region, request.Altitude, request.Varieties, request.Process,
            request.TastingNotes, request.ImageUrl, request.ImageAlt);
        crop.SetProducts(request.ProductIds);

        await repository.AddAsync(crop, cancellationToken);
        return Result<int>.Success(crop.Id);
    }
}
