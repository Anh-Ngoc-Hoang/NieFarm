using MediatR;
using NieFarm.Application.Features.Admin.Activities.Dtos;

namespace NieFarm.Application.Features.Admin.Activities.Queries;

public record GetActivitiesAdminQuery : IRequest<List<ActivityAdminListDto>>;
