using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Account.Profile.Dtos;

namespace NieFarm.Application.Features.Account.Profile.Queries;

public class GetMyProfileQueryHandler(IIdentityService identityService, ICurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, Result<MyProfileDto>>
{
    public async Task<Result<MyProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result<MyProfileDto>.Unauthorized();

        var profile = await identityService.GetCustomerProfileAsync(userId, cancellationToken);
        return profile is null
            ? Result<MyProfileDto>.NotFound()
            : Result<MyProfileDto>.Success(new MyProfileDto(profile.FullName, profile.Email, profile.PhoneNumber));
    }
}
