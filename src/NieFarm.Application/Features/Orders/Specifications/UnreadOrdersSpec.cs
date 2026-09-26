using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>Backs the "new orders" badge in the admin sidebar.</summary>
public sealed class UnreadOrdersSpec : Specification<Order>
{
    public UnreadOrdersSpec() => Query.Where(o => !o.IsRead);
}
