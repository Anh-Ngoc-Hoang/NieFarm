using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Orders.Specifications;

/// <summary>
/// Finds the order that already recorded a gateway transaction (the webhook duplicate check). No
/// includes: only existence matters. The filtered unique index on (Provider, ExternalId) is the
/// backstop.
/// </summary>
public sealed class OrderByPaymentExternalIdSpec : Specification<Order>, ISingleResultSpecification<Order>
{
    public OrderByPaymentExternalIdSpec(string provider, string externalId)
    {
        Query.Where(o => o.PaymentChanges.Any(p => p.Provider == provider && p.ExternalId == externalId));
    }
}
