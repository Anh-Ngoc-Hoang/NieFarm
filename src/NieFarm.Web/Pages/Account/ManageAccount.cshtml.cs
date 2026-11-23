using System.Text.RegularExpressions;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NieFarm.Application.Features.Account.Password.Commands;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Web.Pages.Account;

/// <summary>
/// Endpoints that must run on a real HTTP response because they touch the auth cookie (R-16):
/// change password (D-05) and the session refresh after a profile save (D-06). A Blazor circuit has
/// no response to write a cookie to, and changing the password changes the security stamp, which
/// would otherwise sign this very session out at the next SecurityStampValidator check.
///
/// Antiforgery stays ON (no [IgnoreAntiforgeryToken], Q-16): the form is a Blazor page that renders
/// the token with &lt;AntiforgeryToken /&gt;, which the R-16 spike confirmed validates here.
/// Passwords arrive only in the POST body and never reach a URL, a log or redirect.
/// </summary>
[Authorize]
public class ManageAccountModel(
    ISender sender,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ILogger<ManageAccountModel> logger) : PageModel
{
    private const string ProfilePath = "/tai-khoan/ho-so";

    [BindProperty] public string? CurrentPassword { get; set; }
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }

    // A bare GET (no handler) has nothing to show.
    public IActionResult OnGet() => Redirect(ProfilePath);

    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeMyPasswordCommand(CurrentPassword ?? string.Empty, NewPassword ?? string.Empty, ConfirmPassword ?? string.Empty),
            cancellationToken);

        var userId = userManager.GetUserId(User);

        if (result.IsSuccess)
        {
            // The password change rotated the security stamp: re-issue THIS cookie with the new stamp so
            // this session survives while every other session is signed out at its next stamp check.
            var user = await userManager.GetUserAsync(User);
            if (user is not null)
                await signInManager.RefreshSignInAsync(user);

            logger.LogInformation("Password changed for user {UserId}", userId);
            return Redirect($"{ProfilePath}?trang-thai=doi-mat-khau");
        }

        switch (result.Status)
        {
            case ResultStatus.Forbidden:
                // Locked out by too many wrong current passwords: end the session.
                await signInManager.SignOutAsync();
                logger.LogWarning("Password change locked out user {UserId}", userId);
                return Redirect("/dang-nhap?error=locked");

            case ResultStatus.Unauthorized:
                return Redirect($"/dang-nhap?returnUrl={Uri.EscapeDataString(ProfilePath)}");

            case ResultStatus.Invalid:
                logger.LogInformation("Password change rejected for user {UserId}", userId);
                return Redirect($"{ProfilePath}?loi={ErrorCode(result)}");

            default:
                logger.LogWarning("Password change failed for user {UserId}", userId);
                return Redirect($"{ProfilePath}?loi=khac");
        }
    }

    /// <summary>
    /// Re-issues the current auth cookie so the header shows the name just saved (D-06). Only the
    /// caller's own cookie is refreshed, and only a local path under /tai-khoan is honoured as the
    /// destination (open-redirect guard).
    /// </summary>
    public async Task<IActionResult> OnGetRefreshAsync(string? returnUrl)
    {
        // RefreshSignInAsync alone checks nothing: it would re-issue a fresh cookie (new stamp, new
        // sliding window) to a cookie that is already invalid but still inside SecurityStampValidator's
        // 30-minute interval, for example one stolen before a password change or belonging to an account
        // that has since been locked. So prove the stamp still matches and the account may sign in first.
        var user = await signInManager.ValidateSecurityStampAsync(User);
        if (user is null
            || !await signInManager.CanSignInAsync(user)
            || await userManager.IsLockedOutAsync(user))
        {
            await signInManager.SignOutAsync();
            return Redirect("/dang-nhap");
        }

        await signInManager.RefreshSignInAsync(user);

        return Redirect(IsSafeAccountPath(returnUrl) ? returnUrl! : ProfilePath);
    }

    // The only caller sends /tai-khoan/ho-so?da-luu=1. Accept only plain ASCII lowercase path segments
    // (no "." or "..", no percent-encoding, no non-ASCII) with an optional simple query, nothing else.
    private static readonly Regex SafeAccountPath = new(
        @"^/tai-khoan(/[a-z0-9-]+)*(\?[a-z0-9=&-]*)?\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    private bool IsSafeAccountPath(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl)
        && Url.IsLocalUrl(returnUrl)
        && SafeAccountPath.IsMatch(returnUrl);

    /// <summary>Maps a validation identifier to a fixed whitelist code; anything else is "khac".</summary>
    private static string ErrorCode(Result result) =>
        result.ValidationErrors.FirstOrDefault()?.Identifier switch
        {
            ChangeMyPasswordCommandValidator.CurrentMissingField => "thieu-hien-tai",
            ChangeMyPasswordCommandValidator.CurrentField => "hien-tai",
            ChangeMyPasswordCommandValidator.NewField => "moi",
            ChangeMyPasswordCommandValidator.NewSameAsCurrentField => "trung",
            ChangeMyPasswordCommandValidator.ConfirmField => "xac-nhan",
            _ => "khac"
        };
}
