using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Crops.Specifications;

public sealed class CropByYearSpec : Specification<Crop>, ISingleResultSpecification<Crop>
{
    public CropByYearSpec(int year, int? excludeId = null)
    {
        Query.Where(c => c.Year == year);

        if (excludeId is { } id)
            Query.Where(c => c.Id != id);
    }
}
