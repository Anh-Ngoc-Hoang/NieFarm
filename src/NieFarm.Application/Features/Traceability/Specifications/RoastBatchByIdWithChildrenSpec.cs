using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>
/// One batch with its child rows. Tracked by default for the save command; the edit form's read passes
/// <paramref name="noTracking"/>.
/// </summary>
public sealed class RoastBatchByIdWithChildrenSpec : Specification<RoastBatch>, ISingleResultSpecification<RoastBatch>
{
    public RoastBatchByIdWithChildrenSpec(int id, bool noTracking = false)
    {
        Query.Where(b => b.Id == id)
            .Include(b => b.Certifications)
            .Include(b => b.BrewGuides)
            .AsSplitQuery();

        if (noTracking)
            Query.AsNoTracking();
    }
}
