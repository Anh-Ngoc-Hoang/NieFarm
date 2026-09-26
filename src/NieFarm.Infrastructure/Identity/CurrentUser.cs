using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Infrastructure.Identity;

/// <summary>
/// Resolves the signed-in user for Application handlers.
///
/// AuthenticationStateProvider is the source of truth, NOT IHttpContextAccessor:
/// HttpContext is null for the entire lifetime of a Blazor Server circuit, which is
/// exactly when checkout submits and order-history loads run. The provider is resolved
/// lazily through IServiceProvider so this type stays constructible in scopes that have
/// no component model (Razor Pages, minimal API endpoints), where it falls back to
/// HttpContext and then to null.
/// </summary>
public sealed class CurrentUser(
    IServiceProvider serviceProvider,
    IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var provider = serviceProvider.GetService<AuthenticationStateProvider>();
        if (provider is not null)
        {
            var state = await provider.GetAuthenticationStateAsync();
            var id = state.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(id))
                return id;
        }

        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<string?> GetDisplayNameAsync(CancellationToken cancellationToken = default)
    {
        var provider = serviceProvider.GetService<AuthenticationStateProvider>();
        if (provider is not null)
        {
            var state = await provider.GetAuthenticationStateAsync();
            var name = ResolveDisplayName(state.User);
            if (name is not null)
                return name;
        }

        var httpUser = httpContextAccessor.HttpContext?.User;
        return httpUser is null ? null : ResolveDisplayName(httpUser);
    }

    /// <summary>
    /// The "FullName" claim added by AppUserClaimsPrincipalFactory, falling back to
    /// ClaimTypes.Name, then the local-part of the email, then null.
    /// </summary>
    private static string? ResolveDisplayName(ClaimsPrincipal user)
    {
        var fullName = user.FindFirstValue("FullName");
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName;

        var name = user.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        var email = user.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            var at = email.IndexOf('@');
            return at > 0 ? email[..at] : email;
        }

        return null;
    }
}
