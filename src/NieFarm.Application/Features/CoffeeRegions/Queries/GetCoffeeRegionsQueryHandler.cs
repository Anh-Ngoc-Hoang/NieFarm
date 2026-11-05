using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.CoffeeRegions.Dtos;
using NieFarm.Application.Features.CoffeeRegions.Specifications;
using NieFarm.Application.Features.Crops;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.CoffeeRegions.Queries;

/// <summary>Read repository only, so home can run it in parallel with its other queries.</summary>
public class GetCoffeeRegionsQueryHandler(IReadRepositoryBase<CoffeeRegion> repository)
    : IRequestHandler<GetCoffeeRegionsQuery, List<CoffeeRegionDto>>
{
    public async Task<List<CoffeeRegionDto>> Handle(
        GetCoffeeRegionsQuery request, CancellationToken cancellationToken)
    {
        var regions = await repository.ListAsync(new AllCoffeeRegionsWithAreasSpec(), cancellationToken);

        return regions.Select(r => new CoffeeRegionDto(
            r.Key,
            r.Title,
            r.Tagline,
            CropText.SplitParagraphs(r.Description),
            r.Altitude,
            r.Varieties,
            r.Climate,
            r.FlavorProfile,
            string.IsNullOrWhiteSpace(r.ImageUrl) ? null : r.ImageUrl,
            r.ImageAlt,
            r.Areas
                .OrderBy(a => a.SortOrder)
                .ThenBy(a => a.Id)
                .Select(a => new CoffeeRegionAreaDto(a.Name, a.Note))
                .ToList())).ToList();
    }
}
