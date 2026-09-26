using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Activities.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Activities.Queries;

public class GetActivityForEditQueryHandler(IReadRepositoryBase<Activity> repository)
    : IRequestHandler<GetActivityForEditQuery, ActivityEditDto?>
{
    public async Task<ActivityEditDto?> Handle(
        GetActivityForEditQuery request, CancellationToken cancellationToken)
    {
        var activity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (activity is null)
            return null;

        return new ActivityEditDto(
            activity.Id,
            activity.Title,
            activity.Slug,
            activity.ModalTitle,
            activity.EventDate,
            activity.ImageUrl,
            activity.ImageAlt,
            activity.CardSize,
            activity.Body,
            activity.IsPublished);
    }
}
