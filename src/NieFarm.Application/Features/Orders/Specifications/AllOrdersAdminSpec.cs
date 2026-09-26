using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>Every order with its lines, newest first.</summary>
public sealed class AllOrdersAdminSpec : Specification<Order>
{
    public AllOrdersAdminSpec()
    {
        Query.Include(o => o.Items)
             .OrderByDescending(o => o.CreatedAt)
             .ThenByDescending(o => o.Id);
    }
}
