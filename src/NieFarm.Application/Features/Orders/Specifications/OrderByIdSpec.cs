using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

public sealed class OrderByIdSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public OrderByIdSpec(int id)
    {
        Query.Include(o => o.Items)
             .Where(o => o.Id == id);
    }
}
