using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Activities.Specifications;

/// <summary>Published activities only, newest event first — what the public /hoat-dong page reads.</summary>
public sealed class PublishedActivitiesSpec : Specification<Activity>
{
    public PublishedActivitiesSpec()
    {
        Query.Where(a => a.IsPublished)
             .OrderByDescending(a => a.EventDate)
             .ThenByDescending(a => a.Id);
    }
}
