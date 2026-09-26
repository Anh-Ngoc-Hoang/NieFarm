using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Application.Features.Admin.Accounts.Commands;

public class UpdateAccountCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateAccountCommand, Result>
{
    public Task<Result> Handle(UpdateAccountCommand request, CancellationToken cancellationToken) =>
        identityService.UpdateUserAsync(
            request.Id, request.FullName, request.Password, request.Role,
            request.IsActive, request.PhoneNumber, cancellationToken);
}
