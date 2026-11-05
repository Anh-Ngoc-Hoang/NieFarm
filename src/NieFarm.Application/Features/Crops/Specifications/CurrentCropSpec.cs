using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Crops.Specifications;

/// <summary>The single current crop, if any — what the home page reads.</summary>
public sealed class CurrentCropSpec : Specification<Crop>, ISingleResultSpecification<Crop>
{
    public CurrentCropSpec()
    {
        Query.Where(c => c.IsCurrent)
            .Include(c => c.Products);
    }
}
