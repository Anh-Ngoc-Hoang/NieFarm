using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Partners.Specifications;

/// <summary>Every partner, visible or not, in display order.</summary>
public sealed class AllPartnersAdminSpec : Specification<Partner>
{
    public AllPartnersAdminSpec()
    {
        Query.OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id);
    }
}
