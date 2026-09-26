using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Accounts.Dtos;

namespace NieFarm.Application.Features.Admin.Accounts.Queries;

public class GetAccountForEditQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetAccountForEditQuery, AccountDto?>
{
    public Task<AccountDto?> Handle(GetAccountForEditQuery request, CancellationToken cancellationToken) =>
        identityService.GetAccountAsync(request.Id, cancellationToken);
}
