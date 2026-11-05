using Ardalis.Specification;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.CoffeeRegions.Specifications;

public sealed class CoffeeRegionByKeyWithAreasSpec : Specification<CoffeeRegion>, ISingleResultSpecification<CoffeeRegion>
{
    public CoffeeRegionByKeyWithAreasSpec(CoffeeRegionKey key)
    {
        Query.Where(r => r.Key == key)
            .Include(r => r.Areas);
    }
}
