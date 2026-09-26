using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>
/// One order, but only if it belongs to this customer. Ownership is part of the WHERE
/// clause rather than a post-load check, so there is no code path that can return
/// someone else's order — see .claude/rules/security.md.
/// </summary>
public sealed class MyOrderByIdSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public MyOrderByIdSpec(int id, string customerId)
    {
        Query.Include(o => o.Items)
             .Where(o => o.Id == id && o.CustomerId == customerId);
    }
}
