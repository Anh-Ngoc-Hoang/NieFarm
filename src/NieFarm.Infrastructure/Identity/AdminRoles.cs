namespace NieFarm.Infrastructure.Identity;

public static class AdminRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";

    public static readonly string[] All = [SuperAdmin, Admin];
}
