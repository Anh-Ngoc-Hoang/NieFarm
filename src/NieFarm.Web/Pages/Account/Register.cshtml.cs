using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NieFarm.Application.Features.Account.Commands;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Web.Pages.Account;

/// <summary>
/// Self-service registration endpoint. Lives in a Razor Page (not the Blazor form itself)
/// because signing the new account in immediately needs a real HTTP response.
/// </summary>
public class RegisterModel(
    ISender sender,
    SignInManager<ApplicationUser> signInManager,
    ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string FullName { get; set; } = string.Empty;
    [BindProperty] public string PhoneNumber { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Where the visitor was heading before they were sent to sign in and chose "Đăng ký ngay" instead
    /// (R-16 D-20). Never trusted: see <see cref="SafeReturnUrl"/>.
    /// </summary>
    [BindProperty] public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => Redirect("/dang-ky");

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterCustomerCommand(
            Email, FullName, Password, ConfirmPassword, PhoneNumber), cancellationToken);

        if (result.IsSuccess)
        {
            // Sign in immediately; SignInManager needs this real HTTP response for the cookie.
            await signInManager.PasswordSignInAsync(Email, Password, isPersistent: false, lockoutOnFailure: false);
            // D-03 (R-16): the user id only, never the email.
            logger.LogInformation("Customer registered: user {UserId}", result.Value);
            return LocalRedirect(SafeReturnUrl());
        }

        // ValidationBehavior returns an invalid Result; both shapes carry messages.
        var message = result.ValidationErrors.Select(v => v.ErrorMessage)
            .Concat(result.Errors)
            .FirstOrDefault() ?? "Không thể tạo tài khoản. Vui lòng thử lại.";

        // Status only (D-03): an Identity error description can quote the email or user name.
        logger.LogWarning("Customer registration failed ({ResultStatus})", result.Status);

        return Redirect("/dang-ky"
            + $"?error={Uri.EscapeDataString(message)}"
            + $"&email={Uri.EscapeDataString(Email)}"
            + $"&fullName={Uri.EscapeDataString(FullName)}"
            + $"&phone={Uri.EscapeDataString(PhoneNumber ?? string.Empty)}"
            + ReturnUrlQuery());
    }

    /// <summary>
    /// The post-registration destination: a same-site path or "/". Anything that could leave the site
    /// (an absolute URL, "//host", "/\host", a path that decodes to either) or point at an auth page that
    /// would bounce the new customer (/dang-nhap, /dang-ky, anything under /pages/ such as sign-out) falls
    /// back to the home page. Control characters (a header-splitting newline), spaces and non-ASCII are refused too.
    /// </summary>
    private string SafeReturnUrl()
    {
        var url = ReturnUrl;
        // IsLocalUrl also accepts "~/..." (an app-relative path); a redirect needs a plain "/path".
        if (string.IsNullOrWhiteSpace(url) || url[0] != '/' || !Url.IsLocalUrl(url))
            return "/";

        if (url.Any(c => c <= 0x20 || c > 0x7E || c == '\\'))
            return "/";

        var path = url.Split('?', '#')[0];
        string decoded;
        try
        {
            decoded = Uri.UnescapeDataString(path);
        }
        catch (UriFormatException)
        {
            return "/";
        }

        if (decoded.StartsWith("//", StringComparison.Ordinal) || decoded.Contains('\\'))
            return "/";

        // A "." or ".." segment (also written %2e) would let the browser resolve the path to somewhere the
        // filters below never saw (/a/../pages/..., /dang-ky/.): refuse the lot.
        if (decoded.Split('/').Any(segment => segment is "." or ".."))
            return "/";

        var normalised = decoded.TrimEnd('/');
        if (normalised.Equals("/dang-nhap", StringComparison.OrdinalIgnoreCase)
            || normalised.Equals("/dang-ky", StringComparison.OrdinalIgnoreCase)
            || normalised.Equals("/pages", StringComparison.OrdinalIgnoreCase)
            || normalised.StartsWith("/pages/", StringComparison.OrdinalIgnoreCase))
            return "/";

        return url;
    }

    /// <summary>"&amp;returnUrl=..." for the failure redirect, only when the destination is a real one.</summary>
    private string ReturnUrlQuery()
    {
        var safe = SafeReturnUrl();
        return safe == "/" ? string.Empty : $"&returnUrl={Uri.EscapeDataString(safe)}";
    }
}
