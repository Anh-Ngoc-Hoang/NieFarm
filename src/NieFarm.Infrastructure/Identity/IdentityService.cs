using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Accounts.Dtos;
using NieFarm.Application.Features.Admin.Roles.Dtos;

namespace NieFarm.Infrastructure.Identity;

/// <summary>
/// Keeps ASP.NET Identity behind the <see cref="IIdentityService"/> boundary so the
/// Application layer never references UserManager or IdentityUser directly.
/// </summary>
public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IIdentityService
{
    public async Task<List<AccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken)
    {
        // Staff only. Self-registered customers are not managed here — the admin Accounts
        // page has no paging or search and would be swamped by them.
        var staff = new Dictionary<string, ApplicationUser>(StringComparer.Ordinal);

        foreach (var role in AdminRoles.All)
            foreach (var user in await userManager.GetUsersInRoleAsync(role))
                staff[user.Id] = user;

        var accounts = new List<AccountDto>(staff.Count);
        foreach (var user in staff.Values.OrderBy(u => u.Email))
            accounts.Add(await MapAsync(user));

        return accounts;
    }

    public async Task<AccountDto?> GetAccountAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is null ? null : await MapAsync(user);
    }

    public async Task<Result<string>> CreateUserAsync(
        string email, string fullName, string password, string role,
        string? phoneNumber, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
            return Result<string>.Error("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = fullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return Result<string>.Error(new ErrorList(created.Errors.Select(e => e.Description)));

        if (await roleManager.RoleExistsAsync(role))
            await userManager.AddToRoleAsync(user, role);

        return Result<string>.Success(user.Id);
    }

    public async Task<Result> UpdateUserAsync(
        string userId, string fullName, string? password, string role, bool isActive,
        string? phoneNumber, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.NotFound();

        // Demoting or deactivating the only remaining admin would lock everyone out.
        if (await IsLastAdminAsync(user) && (!isActive || !AdminRoles.All.Contains(role)))
            return Result.Error("This is the last active administrator; keep the account active and in an admin role.");

        user.FullName = fullName.Trim();

        var setPhone = await userManager.SetPhoneNumberAsync(
            user, string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim());
        if (!setPhone.Succeeded)
            return Result.Error(new ErrorList(setPhone.Errors.Select(e => e.Description)));

        // Identity treats a future LockoutEnd as "locked out"; clearing it re-enables sign-in.
        user.LockoutEnabled = !isActive;
        user.LockoutEnd = isActive ? null : DateTimeOffset.MaxValue;

        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
            return Result.Error(new ErrorList(updated.Errors.Select(e => e.Description)));

        if (!string.IsNullOrEmpty(password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, password);
            if (!reset.Succeeded)
                return Result.Error(new ErrorList(reset.Errors.Select(e => e.Description)));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            await userManager.RemoveFromRolesAsync(user, currentRoles);

        if (await roleManager.RoleExistsAsync(role))
            await userManager.AddToRoleAsync(user, role);

        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.NotFound();

        if (await IsLastAdminAsync(user))
            return Result.Error("Cannot delete the last remaining administrator account.");

        var deleted = await userManager.DeleteAsync(user);
        return deleted.Succeeded
            ? Result.Success()
            : Result.Error(new ErrorList(deleted.Errors.Select(e => e.Description)));
    }

    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = roleManager.Roles.OrderBy(r => r.Name).ToList();
        var result = new List<RoleDto>(roles.Count);

        foreach (var role in roles)
        {
            var name = role.Name ?? string.Empty;
            var users = await userManager.GetUsersInRoleAsync(name);
            result.Add(new RoleDto(role.Id, name, users.Count));
        }

        return result;
    }

    public async Task<int> CountAdminsAsync(CancellationToken cancellationToken)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in AdminRoles.All)
        {
            foreach (var user in await userManager.GetUsersInRoleAsync(role))
            {
                if (IsActive(user))
                    seen.Add(user.Id);
            }
        }

        return seen.Count;
    }

    /// <summary>True when removing or disabling this user would leave no active admin behind.</summary>
    private async Task<bool> IsLastAdminAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Any(AdminRoles.All.Contains) || !IsActive(user))
            return false;

        return await CountAdminsAsync(CancellationToken.None) <= 1;
    }

    private static bool IsActive(ApplicationUser user) =>
        user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow;

    public async Task<Result<string>> RegisterCustomerAsync(
        string email, string fullName, string password, string? phoneNumber,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();

        // RequireUniqueEmail is on, so an existing admin account with this email is the
        // *same* account — registering again is a conflict, not a second identity.
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
            return Result<string>.Conflict();

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = fullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            EmailConfirmed = true          // SignIn.RequireConfirmedEmail is false; matches CreateUserAsync
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return Result<string>.Error(new ErrorList(created.Errors.Select(e => e.Description)));

        // No role assignment: there is no Customer role (see §6.1). "Customer" just means
        // "an authenticated account with none of AdminRoles.All" — nothing further to do here.
        return Result<string>.Success(user.Id);
    }

    private async Task<AccountDto> MapAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new AccountDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber,
            roles.FirstOrDefault() ?? string.Empty,
            IsActive(user));
    }
}
