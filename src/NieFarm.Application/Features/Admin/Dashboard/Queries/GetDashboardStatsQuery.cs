using MediatR;
using NieFarm.Application.Features.Admin.Dashboard.Dtos;

namespace NieFarm.Application.Features.Admin.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;
