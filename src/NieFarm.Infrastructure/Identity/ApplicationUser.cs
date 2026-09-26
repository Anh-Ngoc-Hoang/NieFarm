using Microsoft.AspNetCore.Identity;

namespace NieFarm.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
