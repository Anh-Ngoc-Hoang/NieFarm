using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

public sealed class OrderByCodeSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public OrderByCodeSpec(string code) => Query.Where(o => o.Code == code);
}
