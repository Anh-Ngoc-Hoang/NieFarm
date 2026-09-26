using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>Every order belonging to one customer, newest first. Mirrors AllOrdersAdminSpec.</summary>
public sealed class MyOrdersSpec : Specification<Order>
{
    public MyOrdersSpec(string customerId)
    {
        Query.Include(o => o.Items)
             .Where(o => o.CustomerId == customerId)
             .OrderByDescending(o => o.CreatedAt)
             .ThenByDescending(o => o.Id);
    }
}
