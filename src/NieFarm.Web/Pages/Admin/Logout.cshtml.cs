using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Web.Pages.Admin;

public class LogoutModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LogoutModel> logger) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var user = User.Identity?.Name;
        await signInManager.SignOutAsync();
        logger.LogInformation("Admin sign-out for {Email}", user);

        return Redirect("/admin/login");
    }
}
