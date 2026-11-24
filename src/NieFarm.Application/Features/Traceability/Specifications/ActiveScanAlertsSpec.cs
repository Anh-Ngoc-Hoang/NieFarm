using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>
/// Batches whose alert is open: raised, and not acknowledged since. The SQL form of
/// <see cref="RoastBatchScanStat.IsAlertActive"/>; keep the two in step.
/// </summary>
public sealed class ActiveScanAlertsSpec : Specification<RoastBatchScanStat>
{
    public ActiveScanAlertsSpec()
    {
        Query.Where(s => s.AlertRaisedAt != null
                      && (s.AlertAcknowledgedAt == null || s.AlertAcknowledgedAt < s.AlertRaisedAt))
            .AsNoTracking();
    }
}
