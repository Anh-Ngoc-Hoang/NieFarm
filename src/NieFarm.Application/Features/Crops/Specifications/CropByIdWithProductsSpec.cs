using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Crops.Specifications;

public sealed class CropByIdWithProductsSpec : Specification<Crop>, ISingleResultSpecification<Crop>
{
    public CropByIdWithProductsSpec(int id)
    {
        Query.Where(c => c.Id == id)
            .Include(c => c.Products);
    }
}
