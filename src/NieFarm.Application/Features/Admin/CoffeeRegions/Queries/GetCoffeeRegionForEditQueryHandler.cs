using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;
using NieFarm.Application.Features.CoffeeRegions.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Queries;

public class GetCoffeeRegionForEditQueryHandler(IReadRepositoryBase<CoffeeRegion> repository)
    : IRequestHandler<GetCoffeeRegionForEditQuery, CoffeeRegionEditDto?>
{
    public async Task<CoffeeRegionEditDto?> Handle(
        GetCoffeeRegionForEditQuery request, CancellationToken cancellationToken)
    {
        var region = await repository.FirstOrDefaultAsync(
            new CoffeeRegionByKeyWithAreasSpec(request.Key), cancellationToken);
        if (region is null)
            return null;

        return new CoffeeRegionEditDto(
            region.Key,
            region.Title,
            region.Tagline,
            region.Description,
            region.Altitude,
            region.Varieties,
            region.Climate,
            region.FlavorProfile,
            region.ImageUrl,
            region.ImageAlt,
            region.Areas
                .OrderBy(a => a.SortOrder)
                .ThenBy(a => a.Id)
                .Select(a => new CoffeeRegionAreaInput(a.Id, a.Name, a.Note))
                .ToList());
    }
}
