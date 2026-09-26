using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Activities.Specifications;

public sealed class ActivityBySlugSpec : Specification<Activity>, ISingleResultSpecification<Activity>
{
    public ActivityBySlugSpec(string slug, int? excludeId = null)
    {
        Query.Where(a => a.Slug == slug);

        if (excludeId is { } id)
            Query.Where(a => a.Id != id);
    }
}
