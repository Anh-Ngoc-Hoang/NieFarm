using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.CoffeeRegions.Specifications;

/// <summary>Every coffee region with its areas, in map order (North, Central, South).</summary>
public sealed class AllCoffeeRegionsWithAreasSpec : Specification<CoffeeRegion>
{
    public AllCoffeeRegionsWithAreasSpec()
    {
        Query.Include(r => r.Areas)
            .OrderBy(r => r.Key);
    }
}
