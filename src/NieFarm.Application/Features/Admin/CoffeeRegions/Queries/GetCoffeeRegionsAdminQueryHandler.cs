using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.CoffeeRegions.Dtos;
using NieFarm.Application.Features.CoffeeRegions.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.CoffeeRegions.Queries;

public class GetCoffeeRegionsAdminQueryHandler(IReadRepositoryBase<CoffeeRegion> repository)
    : IRequestHandler<GetCoffeeRegionsAdminQuery, List<CoffeeRegionAdminListDto>>
{
    public async Task<List<CoffeeRegionAdminListDto>> Handle(
        GetCoffeeRegionsAdminQuery request, CancellationToken cancellationToken)
    {
        var regions = await repository.ListAsync(new AllCoffeeRegionsWithAreasSpec(), cancellationToken);

        return regions.Select(r => new CoffeeRegionAdminListDto(
            r.Key,
            r.Title,
            r.Tagline,
            r.ImageUrl,
            r.Areas.Count,
            r.UpdatedAt)).ToList();
    }
}
