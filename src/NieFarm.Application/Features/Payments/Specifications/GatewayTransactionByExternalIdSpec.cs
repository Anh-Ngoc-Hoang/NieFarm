using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Payments.Specifications;

/// <summary>Finds the audit row of a gateway transaction by its idempotency key (the webhook duplicate check).</summary>
public sealed class GatewayTransactionByExternalIdSpec : Specification<GatewayTransaction>
{
    public GatewayTransactionByExternalIdSpec(string provider, string externalId)
    {
        Query.Where(t => t.Provider == provider && t.ExternalId == externalId);
    }
}
