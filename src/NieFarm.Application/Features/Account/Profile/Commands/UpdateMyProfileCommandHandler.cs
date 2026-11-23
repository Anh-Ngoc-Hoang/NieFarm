using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Application.Features.Account.Profile.Commands;

public class UpdateMyProfileCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    : IRequestHandler<UpdateMyProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();

        var result = await identityService.UpdateCustomerProfileAsync(
            userId, request.FullName, request.PhoneNumber, cancellationToken);

        if (result.IsSuccess)
            return Result.Success();

        return result.Status == ResultStatus.NotFound
            ? Result.NotFound(AccountMessages.ProfileNotFound)
            : Result.Error(AccountMessages.ProfileSaveFailed);
    }
}
