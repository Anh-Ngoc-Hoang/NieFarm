using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;
using NieFarm.Application.Features.CoffeeRegions.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Commands;

/// <summary>Write repository only — the region is loaded and saved on the same context (data-access.md).</summary>
public class SaveCoffeeRegionCommandHandler(
    IRepositoryBase<CoffeeRegion> repository,
    IImageStorage imageStorage)
    : IRequestHandler<SaveCoffeeRegionCommand, Result>
{
    public async Task<Result> Handle(SaveCoffeeRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await repository.FirstOrDefaultAsync(
            new CoffeeRegionByKeyWithAreasSpec(request.Key), cancellationToken);
        if (region is null)
            return Result.NotFound();

        // Another admin removed an area this form still lists: an expected concurrency outcome,
        // not a crash (SetAreas would otherwise throw on the unknown Id).
        var knownIds = region.Areas.Select(a => a.Id).ToHashSet();
        if (request.Areas.Any(a => a.Id is { } id && !knownIds.Contains(id)))
            return Result.Error("One or more areas were changed elsewhere. Reload and try again.");

        var previousImageUrl = region.ImageUrl;

        region.UpdateContent(
            request.Title, request.Tagline, request.Description,
            request.Altitude, request.Varieties, request.Climate, request.FlavorProfile,
            request.ImageUrl, request.ImageAlt);
        region.SetAreas(request.Areas.Select(a => (a.Id, a.Name, a.Note)).ToList());

        await repository.UpdateAsync(region, cancellationToken);

        if (CoffeeRegionImageLimits.IsManagedImage(previousImageUrl)
            && !string.Equals(previousImageUrl, region.ImageUrl, StringComparison.OrdinalIgnoreCase))
        {
            await imageStorage.DeleteManyAsync([previousImageUrl], cancellationToken);
        }

        return Result.Success();
    }
}
