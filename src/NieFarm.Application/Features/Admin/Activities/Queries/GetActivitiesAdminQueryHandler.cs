using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Activities.Specifications;
using NieFarm.Application.Features.Admin.Activities.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Activities.Queries;

public class GetActivitiesAdminQueryHandler(IReadRepositoryBase<Activity> repository)
    : IRequestHandler<GetActivitiesAdminQuery, List<ActivityAdminListDto>>
{
    public async Task<List<ActivityAdminListDto>> Handle(
        GetActivitiesAdminQuery request, CancellationToken cancellationToken)
    {
        var activities = await repository.ListAsync(new AllActivitiesAdminSpec(), cancellationToken);

        return activities.Select(a => new ActivityAdminListDto(
            a.Id,
            a.Title,
            a.Slug,
            a.EventDate,
            a.ImageUrl,
            a.IsPublished)).ToList();
    }
}
