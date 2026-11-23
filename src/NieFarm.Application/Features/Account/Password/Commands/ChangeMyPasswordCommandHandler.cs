using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Application.Features.Account.Password.Commands;

public class ChangeMyPasswordCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    : IRequestHandler<ChangeMyPasswordCommand, Result>
{
    public async Task<Result> Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId is null)
            return Result.Unauthorized();

        var result = await identityService.ChangePasswordAsync(
            userId, request.Current, request.New, cancellationToken);

        if (result.IsSuccess)
            return Result.Success();

        // A deleted account with a still-valid cookie is treated as signed out.
        if (result.Status == ResultStatus.NotFound)
            return Result.Unauthorized();

        var codes = result.Errors.ToList();

        if (codes.Contains(AccountErrorCodes.LockedOut))
            return Result.Forbidden();

        if (codes.Contains(AccountErrorCodes.PasswordMismatch))
            return Invalid(ChangeMyPasswordCommandValidator.CurrentField, AccountMessages.CurrentPasswordWrong);

        // Identity's own password-policy codes (PasswordTooShort, PasswordRequiresDigit, ...): the new
        // password was rejected (the validator mirrors the policy, so this is rare). The code is never echoed.
        if (codes.Any(IsPasswordPolicyCode))
            return Invalid(ChangeMyPasswordCommandValidator.NewField, AccountMessages.NewPasswordRejected);

        // Anything else (ConcurrencyFailure, SaveFailed, ...) is not the customer's mistake.
        return Result.Error(AccountMessages.PasswordChangeFailed);
    }

    private static bool IsPasswordPolicyCode(string code) =>
        code == "PasswordTooShort" || code.StartsWith("PasswordRequires", StringComparison.Ordinal);

    private static Result Invalid(string identifier, string message) =>
        Result.Invalid(new ValidationError { Identifier = identifier, ErrorMessage = message });
}
