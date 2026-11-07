using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>
/// Write-load variant of <see cref="MyOrderByIdSpec"/> for the customer cancel: same ownership
/// WHERE, but no Includes. The cancel only appends one history row, so loading (and then
/// re-saving) every item and every immutable history row would be pointless extra UPDATEs.
/// </summary>
public sealed class MyOrderForCancelSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public MyOrderForCancelSpec(int id, string customerId)
    {
        Query.Where(o => o.Id == id && o.CustomerId == customerId);
    }
}
