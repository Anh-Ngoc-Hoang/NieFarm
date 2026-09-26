using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Application.Features.Admin.Accounts.Commands;

public class CreateAccountCommandHandler(IIdentityService identityService)
    : IRequestHandler<CreateAccountCommand, Result<string>>
{
    public Task<Result<string>> Handle(CreateAccountCommand request, CancellationToken cancellationToken) =>
        identityService.CreateUserAsync(
            request.Email, request.FullName, request.Password, request.Role,
            request.PhoneNumber, cancellationToken);
}
