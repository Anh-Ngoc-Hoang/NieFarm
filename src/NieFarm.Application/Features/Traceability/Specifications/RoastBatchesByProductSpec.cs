using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>Every batch of one product; deleting a product that still has batches is refused.</summary>
public sealed class RoastBatchesByProductSpec : Specification<RoastBatch>
{
    public RoastBatchesByProductSpec(int productId)
    {
        Query.Where(b => b.ProductId == productId).AsNoTracking();
    }
}
