using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>Used by the save command to rule out a (very unlikely) generated-code collision.</summary>
public sealed class RoastBatchCodeExistsSpec : Specification<RoastBatch>
{
    public RoastBatchCodeExistsSpec(string code)
    {
        Query.Where(b => b.Code == code).AsNoTracking();
    }
}
