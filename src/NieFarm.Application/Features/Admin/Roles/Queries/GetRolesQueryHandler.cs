using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Roles.Dtos;

namespace NieFarm.Application.Features.Admin.Roles.Queries;

public class GetRolesQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    public Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) =>
        identityService.GetRolesAsync(cancellationToken);
}
