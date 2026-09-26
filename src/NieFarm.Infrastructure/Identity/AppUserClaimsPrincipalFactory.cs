using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace NieFarm.Infrastructure.Identity;

/// <summary>
/// Adds the display name to the principal so the admin topbar can greet the user
/// without a round trip to the database on every render.
/// </summary>
public class AppUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.FullName))
            identity.AddClaim(new Claim("FullName", user.FullName));

        return identity;
    }
}
