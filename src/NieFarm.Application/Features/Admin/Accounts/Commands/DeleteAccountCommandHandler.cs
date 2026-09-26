using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Application.Features.Admin.Accounts.Commands;

public class DeleteAccountCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await identityService.GetAccountAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.NotFound();

        // Guard against locking everyone out of the admin.
        if (await identityService.CountAdminsAsync(cancellationToken) <= 1)
            return Result.Error("Cannot delete the last remaining administrator account.");

        return await identityService.DeleteUserAsync(request.Id, cancellationToken);
    }
}
