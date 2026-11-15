using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>Same as OrderForPaymentSpec but addressed by order code (gateway webhook). Items included.</summary>
public sealed class OrderForPaymentByCodeSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public OrderForPaymentByCodeSpec(string code)
    {
        Query.Include(o => o.Items)
             .Where(o => o.Code == code);
    }
}
