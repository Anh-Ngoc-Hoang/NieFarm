using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Accounts.Dtos;

namespace NieFarm.Application.Features.Admin.Accounts.Queries;

public class GetAccountsQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetAccountsQuery, List<AccountDto>>
{
    public Task<List<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken) =>
        identityService.GetAllAccountsAsync(cancellationToken);
}
