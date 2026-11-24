using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.RoastBatches.Queries;

public class GetActiveScanAlertCountQueryHandler(IReadRepositoryBase<RoastBatchScanStat> repository)
    : IRequestHandler<GetActiveScanAlertCountQuery, int>
{
    public Task<int> Handle(GetActiveScanAlertCountQuery request, CancellationToken cancellationToken) =>
        repository.CountAsync(new ActiveScanAlertsSpec(), cancellationToken);
}
