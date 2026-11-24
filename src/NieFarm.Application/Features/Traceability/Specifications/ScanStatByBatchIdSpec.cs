using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>One batch's counter row, tracked (the acknowledge command writes only its alert column).</summary>
public sealed class ScanStatByBatchIdSpec : Specification<RoastBatchScanStat>, ISingleResultSpecification<RoastBatchScanStat>
{
    public ScanStatByBatchIdSpec(int batchId)
    {
        Query.Where(s => s.BatchId == batchId);
    }
}
