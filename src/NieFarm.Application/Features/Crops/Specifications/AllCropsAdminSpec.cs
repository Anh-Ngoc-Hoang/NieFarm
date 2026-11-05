using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Crops.Specifications;

/// <summary>Every crop, newest year first.</summary>
public sealed class AllCropsAdminSpec : Specification<Crop>
{
    public AllCropsAdminSpec()
    {
        Query.Include(c => c.Products)
            .OrderByDescending(c => c.Year);
    }
}
