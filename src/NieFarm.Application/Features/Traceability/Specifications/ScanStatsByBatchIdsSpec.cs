using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>
/// Counter rows for the given batches. Tracked by default because the flush handler mutates them; the admin list
/// passes <paramref name="noTracking"/>.
/// </summary>
public sealed class ScanStatsByBatchIdsSpec : Specification<RoastBatchScanStat>
{
    public ScanStatsByBatchIdsSpec(IReadOnlyCollection<int> batchIds, bool noTracking = false)
    {
        Query.Where(s => batchIds.Contains(s.BatchId));

        if (noTracking)
            Query.AsNoTracking();
    }
}
