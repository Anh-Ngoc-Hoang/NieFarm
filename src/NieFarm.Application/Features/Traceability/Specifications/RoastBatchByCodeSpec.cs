using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>
/// The public page's single read: a batch by its normalised code with the product and child rows, no tracking.
/// An unpublished batch only matches when <paramref name="includeUnpublished"/> (an admin preview).
/// </summary>
public sealed class RoastBatchByCodeSpec : Specification<RoastBatch>, ISingleResultSpecification<RoastBatch>
{
    public RoastBatchByCodeSpec(string code, bool includeUnpublished)
    {
        Query.Where(b => b.Code == code && (includeUnpublished || b.IsPublished))
            .Include(b => b.Product)
            .Include(b => b.Certifications)
            .Include(b => b.BrewGuides)
            .AsSplitQuery()
            .AsNoTracking();
    }
}
