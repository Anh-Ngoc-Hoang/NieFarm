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
[IgnoreAntiforgeryToken]
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

    public IActionResult OnGet() => Redirect("/dang-ky");

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterCustomerCommand(
            Email, FullName, Password, ConfirmPassword, PhoneNumber), cancellationToken);

        if (result.IsSuccess)
        {
            // Sign in immediately; SignInManager needs this real HTTP response for the cookie.
            await signInManager.PasswordSignInAsync(Email, Password, isPersistent: false, lockoutOnFailure: false);
            logger.LogInformation("Customer registered: {Email}", Email);
            return Redirect("/");
        }

        // ValidationBehavior returns an invalid Result; both shapes carry messages.
        var message = result.ValidationErrors.Select(v => v.ErrorMessage)
            .Concat(result.Errors)
            .FirstOrDefault() ?? "Không thể tạo tài khoản. Vui lòng thử lại.";

        logger.LogWarning("Customer registration failed for {Email}: {Message}", Email, message);

        return Redirect("/dang-ky"
            + $"?error={Uri.EscapeDataString(message)}"
            + $"&email={Uri.EscapeDataString(Email)}"
            + $"&fullName={Uri.EscapeDataString(FullName)}"
            + $"&phone={Uri.EscapeDataString(PhoneNumber ?? string.Empty)}");
    }
}
