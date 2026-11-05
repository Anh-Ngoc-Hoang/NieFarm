using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Partners.Specifications;

/// <summary>Visible partners only, in display order — what the home page partner band reads.</summary>
public sealed class VisiblePartnersSpec : Specification<Partner>
{
    public VisiblePartnersSpec()
    {
        Query.Where(p => p.IsVisible)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Take(24);
    }
}
