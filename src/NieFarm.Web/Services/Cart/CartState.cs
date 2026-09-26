using MediatR;
using NieFarm.Application.Features.Carts.Commands;
using NieFarm.Application.Features.Carts.Queries;

namespace NieFarm.Web.Services.Cart;

/// <summary>
/// Scoped (per-circuit) state holding the live cart item count, used by SiteHeader's badge.
/// Modelled on OfficeCoffee's CartState and on NieFarm's own NewOrderCountState (same OnChange
/// event, same swallow-and-continue posture — a badge is never worth breaking the shell over).
///
/// Must only be invoked from interactive rendering (e.g. OnAfterRenderAsync(firstRender: true)
/// or gated on RendererInfo.IsInteractive) — the whole app prerenders first, and the guest-cart
/// merge inside InitializeCoreAsync is a write that must not run twice.
/// </summary>
public sealed class CartState(ISender sender)
{
    private Task? _initialization;

    public int Count { get; private set; }

    public event Action? OnChange;

    /// <summary>
    /// Runs once per circuit: merges a guest cart into the signed-in account, then loads the
    /// count. Concurrent callers await the same Task rather than racing past a bool guard (the
    /// pre-merge cart would otherwise leak to a caller that arrives while the first
    /// initialization is still in flight).
    /// </summary>
    public Task EnsureInitializedAsync(string? userId, string? anonymousId)
        => _initialization ??= InitializeCoreAsync(userId, anonymousId);

    private async Task InitializeCoreAsync(string? userId, string? anonymousId)
    {
        try
        {
            // Strictly sequential — both touch the shared scoped DbContext (data-access.md).
            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(anonymousId))
                await sender.Send(new MergeGuestCartCommand(anonymousId, userId));

            Count = await sender.Send(new GetCartItemCountQuery(anonymousId));
        }
        catch
        {
            // A badge is never worth breaking the shell over — degrade to a zero count.
            Count = 0;
        }

        OnChange?.Invoke();
    }

    /// <summary>Sets the count directly — used by cart commands that already return the
    /// rebuilt cart, avoiding an extra query round trip.</summary>
    public void Set(int count)
    {
        Count = count;
        OnChange?.Invoke();
    }

    public async Task RefreshAsync(string? anonymousId)
    {
        try
        {
            Count = await sender.Send(new GetCartItemCountQuery(anonymousId));
        }
        catch
        {
            Count = 0;
        }

        OnChange?.Invoke();
    }
}
