using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Account;
using NieFarm.Application.Features.Account.Profile.Dtos;
using NieFarm.Application.Features.Admin.Accounts.Dtos;
using NieFarm.Application.Features.Admin.Roles.Dtos;
using NieFarm.Infrastructure.Data;

namespace NieFarm.Infrastructure.Identity;

/// <summary>
/// Keeps ASP.NET Identity behind the <see cref="IIdentityService"/> boundary so the
/// Application layer never references UserManager or IdentityUser directly.
/// </summary>
public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    AppDbContext db) : IIdentityService
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

    public async Task<CustomerProfileDto?> GetCustomerProfileAsync(string userId, CancellationToken cancellationToken)
    {
        // A no-tracking projection: the scoped context lives as long as a Blazor circuit, so a tracked
        // read could keep serving a stale name or phone after another request changed the row.
        return await userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new CustomerProfileDto(u.FullName, u.Email ?? string.Empty, u.PhoneNumber))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result> UpdateCustomerProfileAsync(
        string userId, string fullName, string? phoneNumber, CancellationToken cancellationToken)
    {
        // The user store swallows a failed UpdateAsync (for example ConcurrencyFailure) and leaves the
        // entity Modified in this scoped context, which in Blazor Server lives as long as the circuit; the
        // next checkout would then fail in DiscardTrackedEntities ("unsaved changes"). So: refuse to run
        // over someone else's pending changes, never read a cached copy of this user, and always detach
        // exactly this user afterwards (never the whole context). The same applies to ChangePasswordAsync.
        if (!TryPrepareUserLoad(userId))
            return Result.Error(new ErrorList([AccountErrorCodes.SaveFailed]));

        ApplicationUser? user = null;
        try
        {
            user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.NotFound();

            user.FullName = fullName.Trim();

            // Assign the property and use UpdateAsync, NOT SetPhoneNumberAsync: SetPhoneNumberAsync also
            // updates the security stamp, which would let SecurityStampValidator sign this very session
            // out at its next check (R-16 D-06). Email is never touched here (D-07).
            user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

            var updated = await userManager.UpdateAsync(user);
            return updated.Succeeded
                ? Result.Success()
                : Result.Error(new ErrorList([AccountErrorCodes.SaveFailed]));
        }
        finally
        {
            DetachUser(user);
        }
    }

    public async Task<Result> ChangePasswordAsync(
        string userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        if (!TryPrepareUserLoad(userId))
            return Result.Error(new ErrorList([AccountErrorCodes.SaveFailed]));

        ApplicationUser? user = null;
        try
        {
            user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.NotFound();

            // A locked-out account may not keep guessing the current password through this form.
            if (await userManager.IsLockedOutAsync(user))
                return Result.Error(new ErrorList([AccountErrorCodes.LockedOut]));

            var changed = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (changed.Succeeded)
            {
                await userManager.ResetAccessFailedCountAsync(user);
                return Result.Success();
            }

            var codes = changed.Errors.Select(e => e.Code).ToList();

            if (codes.Contains(AccountErrorCodes.PasswordMismatch))
            {
                // Counts toward lockout, so this form cannot be used to brute-force the password.
                await userManager.AccessFailedAsync(user);

                return Result.Error(new ErrorList([
                    await userManager.IsLockedOutAsync(user)
                        ? AccountErrorCodes.LockedOut
                        : AccountErrorCodes.PasswordMismatch]));
            }

            return Result.Error(new ErrorList(codes));
        }
        finally
        {
            DetachUser(user);
        }
    }

    /// <summary>
    /// Readies the scoped context for a customer write: false when it holds pending changes that belong
    /// to some other operation (those are never silently discarded, so the caller reports a failure);
    /// otherwise any already-tracked copy of this user is detached so the next load reads the database,
    /// not a stale cached instance.
    /// </summary>
    private bool TryPrepareUserLoad(string userId)
    {
        if (db.ChangeTracker.HasChanges())
            return false;

        foreach (var entry in db.ChangeTracker.Entries<ApplicationUser>().Where(e => e.Entity.Id == userId).ToList())
            entry.State = EntityState.Detached;

        return true;
    }

    /// <summary>Detaches just this user (also when a failed save left it Modified); the rest of the context is untouched.</summary>
    private void DetachUser(ApplicationUser? user)
    {
        if (user is not null)
            db.Entry(user).State = EntityState.Detached;
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
