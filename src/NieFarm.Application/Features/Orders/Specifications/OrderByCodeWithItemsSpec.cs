using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>
/// Loads an order and its lines by code only. The phone check happens after loading, on purpose:
/// stored phones are free-form so they are compared in normalised form by the handler.
/// </summary>
public sealed class OrderByCodeWithItemsSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public OrderByCodeWithItemsSpec(string code) => Query.Include(o => o.Items).Where(o => o.Code == code);
}
