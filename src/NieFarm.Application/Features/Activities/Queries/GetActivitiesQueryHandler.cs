using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Activities.Dtos;
using NieFarm.Application.Features.Activities.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Activities.Queries;

public class GetActivitiesQueryHandler(IReadRepositoryBase<Activity> repository)
    : IRequestHandler<GetActivitiesQuery, List<ActivityDto>>
{
    public async Task<List<ActivityDto>> Handle(
        GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await repository.ListAsync(new PublishedActivitiesSpec(), cancellationToken);

        return activities.Select(a => new ActivityDto(
            a.Id,
            a.Slug,
            a.Title,
            string.IsNullOrWhiteSpace(a.ModalTitle) ? a.Title : a.ModalTitle,
            a.EventDate,
            a.ImageUrl,
            a.ImageAlt,
            a.CardSize,
            a.Body)).ToList();
    }
}
