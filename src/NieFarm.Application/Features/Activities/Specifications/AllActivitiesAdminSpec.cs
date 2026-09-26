using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Activities.Specifications;

/// <summary>Every activity, published or not, newest event first.</summary>
public sealed class AllActivitiesAdminSpec : Specification<Activity>
{
    public AllActivitiesAdminSpec()
    {
        Query.OrderByDescending(a => a.EventDate)
             .ThenByDescending(a => a.Id);
    }
}
