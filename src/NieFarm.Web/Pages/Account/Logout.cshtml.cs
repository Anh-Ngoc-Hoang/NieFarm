using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Web.Pages.Account;

public class LogoutModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LogoutModel> logger) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        // D-03 (R-16): the user id only, never the email (Identity.Name is the email here).
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await signInManager.SignOutAsync();
        logger.LogInformation("Customer sign-out for user {UserId}", userId);

        return Redirect("/");
    }
}
