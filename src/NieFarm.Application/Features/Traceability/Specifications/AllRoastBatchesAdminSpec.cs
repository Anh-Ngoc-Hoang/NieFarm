using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability.Specifications;

/// <summary>
/// The admin list: every batch (published or not) with its product, newest roast first. The optional search is a
/// "contains" on the product name or on the code (typed with or without dashes, any case).
/// </summary>
public sealed class AllRoastBatchesAdminSpec : Specification<RoastBatch>
{
    public AllRoastBatchesAdminSpec(string? productTerm, string? codeTerm)
    {
        Query.Include(b => b.Product).AsNoTracking();

        if (productTerm is not null && codeTerm is not null)
            Query.Where(b => b.Product!.Name.Contains(productTerm) || b.Code.Contains(codeTerm));
        else if (productTerm is not null)
            Query.Where(b => b.Product!.Name.Contains(productTerm));

        Query.OrderByDescending(b => b.RoastDate).ThenByDescending(b => b.Id);
    }
}
