using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;
using NieFarm.Application.Features.Traceability;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.RoastBatches.Queries;

public class GetRoastBatchLabelQueryHandler(IReadRepositoryBase<RoastBatch> repository)
    : IRequestHandler<GetRoastBatchLabelQuery, RoastBatchLabelDto?>
{
    public async Task<RoastBatchLabelDto?> Handle(GetRoastBatchLabelQuery request, CancellationToken cancellationToken)
    {
        var batch = await repository.FirstOrDefaultAsync(new RoastBatchLabelSpec(request.Id), cancellationToken);
        if (batch is null)
            return null;

        return new RoastBatchLabelDto(
            batch.Code,
            BatchCode.Format(batch.Code),
            batch.Product?.Name ?? string.Empty,
            batch.RoastDate,
            batch.NetWeightGrams);
    }
}

/// <summary>A batch with its product, no tracking. Published or not.</summary>
internal sealed class RoastBatchLabelSpec : Specification<RoastBatch>, ISingleResultSpecification<RoastBatch>
{
    public RoastBatchLabelSpec(int id)
    {
        Query.Where(b => b.Id == id).Include(b => b.Product).AsNoTracking();
    }
}
