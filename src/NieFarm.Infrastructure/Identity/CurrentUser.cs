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
/// HttpContext and then to null (see GetProviderUserAsync for why that fallback is explicit).
/// </summary>
public sealed class CurrentUser(
    IServiceProvider serviceProvider,
    IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var providerUser = await GetProviderUserAsync();
        var id = providerUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(id))
            return id;

        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<string?> GetDisplayNameAsync(CancellationToken cancellationToken = default)
    {
        var providerUser = await GetProviderUserAsync();
        var name = providerUser is null ? null : ResolveDisplayName(providerUser);
        if (name is not null)
            return name;

        var httpUser = httpContextAccessor.HttpContext?.User;
        return httpUser is null ? null : ResolveDisplayName(httpUser);
    }

    /// <summary>
    /// The user known to the Blazor circuit, or null when there is no component scope. In a Razor
    /// Page request ServerAuthenticationStateProvider is registered but THROWS
    /// InvalidOperationException from GetAuthenticationStateAsync rather than returning null
    /// (verified by the R-16 spike), so that case must be caught here and left to the HttpContext
    /// fallback — otherwise any command sent from a PageModel (e.g. ChangeMyPasswordCommand)
    /// would fail. Behaviour inside a circuit is unchanged.
    /// </summary>
    private async Task<ClaimsPrincipal?> GetProviderUserAsync()
    {
        var provider = serviceProvider.GetService<AuthenticationStateProvider>();
        if (provider is null)
            return null;

        try
        {
            return (await provider.GetAuthenticationStateAsync()).User;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
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
