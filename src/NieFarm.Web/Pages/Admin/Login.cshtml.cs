using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Web.Pages.Admin;

/// <summary>
/// Sign-in endpoint for the Blazor login form. It lives in a Razor Page because
/// SignInManager has to write the auth cookie onto a real HTTP response.
/// </summary>
[IgnoreAntiforgeryToken]
public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public bool RememberMe { get; set; }

    public IActionResult OnGet() => Redirect("/admin/login");

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await signInManager.PasswordSignInAsync(
            Email, Password, RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Admin sign-in succeeded for {Email}", Email);
            return Redirect("/admin");
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("Admin sign-in blocked: {Email} is locked out", Email);
            return Redirect("/admin/login?error=locked");
        }

        logger.LogWarning("Admin sign-in failed for {Email}", Email);
        return Redirect("/admin/login?error=1");
    }
}
