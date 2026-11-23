using FluentValidation;

namespace NieFarm.Application.Features.Account.Password.Commands;

public class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    /// <summary>Identifiers the Razor Page maps to a fixed redirect code; keep them in step.</summary>
    public const string CurrentField = "Current";
    public const string CurrentMissingField = "CurrentMissing";
    public const string NewField = "New";
    public const string NewSameAsCurrentField = "NewSameAsCurrent";
    public const string ConfirmField = "Confirm";

    public ChangeMyPasswordCommandValidator()
    {
        // An empty current password is "missing", not "wrong": its own identifier, which the Razor Page maps
        // to ?loi=thieu-hien-tai and the profile page to "Vui lòng nhập mật khẩu hiện tại.".
        RuleFor(x => x.Current)
            .NotEmpty().WithMessage(AccountMessages.CurrentPasswordRequired)
            .OverridePropertyName(CurrentMissingField);

        // Same identifier (Current) as the handler's "wrong current password", so the page shows the
        // matching text: a value beyond the length cap is reported as not matching (and never reaches the hasher).
        RuleFor(x => x.Current)
            .MaximumLength(PasswordRules.MaxLength).WithMessage(AccountMessages.CurrentPasswordWrong);

        RuleFor(x => x.New)
            .NotEmpty().WithMessage(AccountMessages.NewPasswordRequired)
            .MaximumLength(PasswordRules.MaxLength).WithMessage(AccountMessages.NewPasswordRejected)
            .MeetsPasswordPolicy();

        // A separate chain with its own identifier, so the page can tell "too weak" from "unchanged".
        RuleFor(x => x.New)
            .NotEqual(x => x.Current).WithMessage(AccountMessages.NewPasswordSameAsCurrent)
            .When(x => !string.IsNullOrEmpty(x.New) && !string.IsNullOrEmpty(x.Current))
            .OverridePropertyName(NewSameAsCurrentField);

        RuleFor(x => x.Confirm)
            .Equal(x => x.New).WithMessage(AccountMessages.ConfirmMismatch);
    }
}
