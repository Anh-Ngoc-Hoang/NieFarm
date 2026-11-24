using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Traceability.Dtos;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Queries;

public class GetBatchTraceQueryHandler(
    IReadRepositoryBase<RoastBatch> repository,
    IBatchTraceCache cache)
    : IRequestHandler<GetBatchTraceQuery, BatchTraceDto?>
{
    public async Task<BatchTraceDto?> Handle(GetBatchTraceQuery request, CancellationToken cancellationToken)
    {
        // A wrong shape never reaches the database.
        if (!BatchCode.TryNormalise(request.Code, out var code))
            return null;

        if (!request.IncludeUnpublished && cache.TryGet(code, out var cached))
            return cached;

        var batch = await repository.FirstOrDefaultAsync(
            new RoastBatchByCodeSpec(code, request.IncludeUnpublished), cancellationToken);
        if (batch is null)
            return null;

        var dto = BatchTraceMapper.ToDto(batch);

        // Only a found, published, non-preview result is cached (never a miss).
        if (batch.IsPublished && !request.IncludeUnpublished)
            cache.Set(code, dto);

        return dto;
    }
}
