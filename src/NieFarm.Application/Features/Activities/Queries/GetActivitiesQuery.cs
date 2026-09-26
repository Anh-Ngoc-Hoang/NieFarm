using MediatR;
using NieFarm.Application.Features.Activities.Dtos;

namespace NieFarm.Application.Features.Activities.Queries;

/// <summary>
/// A bare list, not Result&lt;List&lt;ActivityDto&gt;&gt;, because it has no failure mode
/// (mirrors GetCartQuery/GetProvincesQuery). Public content — no scoping, no ICurrentUser.
/// </summary>
public record GetActivitiesQuery : IRequest<List<ActivityDto>>;
