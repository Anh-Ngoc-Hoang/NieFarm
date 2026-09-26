using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NieFarm.Application.Features.Carts.Commands;
using NieFarm.Infrastructure.Identity;
using NieFarm.Web.Services.Cart;

namespace NieFarm.Web.Pages.Account;

/// <summary>
/// Sign-in endpoint for the public customer login form. It lives in a Razor Page because
/// SignInManager has to write the auth cookie onto a real HTTP response.
/// </summary>
[IgnoreAntiforgeryToken]
public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ISender sender,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public bool RememberMe { get; set; }
    [BindProperty] public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => Redirect("/dang-nhap");

    public async Task<IActionResult> OnPostAsync()
    {
        // Open-redirect guard: only same-site paths are ever honoured.
        var safeReturn = !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : "/";

        var result = await signInManager.PasswordSignInAsync(Email, Password, RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Customer sign-in succeeded for {Email}", Email);
            await TryMergeGuestCartAsync();
            return Redirect(safeReturn);
        }

        var query = $"?error={(result.IsLockedOut ? "locked" : "1")}" +
                    $"&email={Uri.EscapeDataString(Email)}" +
                    $"&returnUrl={Uri.EscapeDataString(safeReturn)}";
        logger.LogWarning("Customer sign-in failed for {Email}", Email);
        return Redirect("/dang-nhap" + query);
    }

    /// <summary>
    /// Merges the guest cart (identified by the nf_cart cookie) into the user's cart on
    /// successful sign-in. Best-effort — a merge failure must never block sign-in. Passes the
    /// user id explicitly (D14a) rather than reaching for ICurrentUser: this runs in a Razor
    /// Page request scope, where ICurrentUser's AuthenticationStateProvider-first resolution
    /// has no state set by any component.
    /// </summary>
    private async Task TryMergeGuestCartAsync()
    {
        try
        {
            var anonymousId = Request.Cookies[CartCookie.Name];
            if (string.IsNullOrWhiteSpace(anonymousId))
                return;

            var userId = (await userManager.FindByEmailAsync(Email))?.Id;
            if (!string.IsNullOrWhiteSpace(userId))
                await sender.Send(new MergeGuestCartCommand(anonymousId, userId), HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to merge guest cart on sign-in for {Email}", Email);
        }
    }
}
