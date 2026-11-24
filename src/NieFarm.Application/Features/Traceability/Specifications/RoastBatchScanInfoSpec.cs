using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>The two columns the scan flush needs per batch.</summary>
public sealed record RoastBatchScanInfo(int Id, int? PackCount);

/// <summary>Existing batches among the given ids with their pack count (admin only data, used for the alert rule).</summary>
public sealed class RoastBatchScanInfoSpec : Specification<RoastBatch, RoastBatchScanInfo>
{
    public RoastBatchScanInfoSpec(IReadOnlyCollection<int> ids)
    {
        Query.Where(b => ids.Contains(b.Id)).AsNoTracking();
        Query.Select(b => new RoastBatchScanInfo(b.Id, b.PackCount));
    }
}
