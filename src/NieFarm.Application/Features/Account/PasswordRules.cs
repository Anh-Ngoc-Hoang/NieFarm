using FluentValidation;

namespace NieFarm.Application.Features.Account;

/// <summary>
/// The one password policy, shared by registration and change-password so they can never drift
/// apart. It mirrors the Identity options in Infrastructure (length 8, upper, lower, digit).
/// Presence is checked by the caller (its message differs: "mật khẩu" vs "mật khẩu mới").
/// </summary>
public static class PasswordRules
{
    public const int MinLength = 8;

    /// <summary>Upper bound so an absurdly long value never reaches the password hasher.</summary>
    public const int MaxLength = 128;

    public static IRuleBuilderOptions<T, string> MeetsPasswordPolicy<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .MinimumLength(MinLength).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất một chữ hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất một chữ thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất một chữ số.");
}
