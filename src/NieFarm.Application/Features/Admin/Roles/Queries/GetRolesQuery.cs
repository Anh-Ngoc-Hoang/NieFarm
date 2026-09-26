using MediatR;
using NieFarm.Application.Features.Admin.Roles.Dtos;

namespace NieFarm.Application.Features.Admin.Roles.Queries;

public record GetRolesQuery : IRequest<List<RoleDto>>;
