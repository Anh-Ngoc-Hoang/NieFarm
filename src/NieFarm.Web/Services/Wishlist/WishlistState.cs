using MediatR;
using NieFarm.Application.Features.Wishlist.Commands;
using NieFarm.Application.Features.Wishlist.Queries;
using NieFarm.Web.Services.Cart;

namespace NieFarm.Web.Services.Wishlist;

public enum WishlistOutcome
{
    /// <summary>The change was saved; <see cref="WishlistState.Contains"/> now reflects it.</summary>
    Done,

    /// <summary>An anonymous visitor: the caller sends them to sign in (R-16 D-20).</summary>
    RequiresSignIn,

    /// <summary>Nothing changed (or the outcome is unknown and the state was refreshed from the database).</summary>
    Failed,
}

public readonly record struct WishlistToggleResult(WishlistOutcome Outcome, string? Error = null);

/// <summary>
/// Scoped (per-circuit) holder of the signed-in customer's saved product ids, shared by every heart
/// button on the page (R-16 D-21). It is deliberately NOT part of <c>ProductCardDto</c>: that record is
/// persisted across prerender and does not depend on who is looking.
///
/// Reads: one <see cref="GetMyWishlistProductIdsQuery"/> per circuit (a read repository, so a fresh
/// context and no N+1 across the dozen cards on a page). Every card awaits the same Task, as with
/// <see cref="CartState"/>. Must only be started from interactive rendering. A failed load is not final:
/// <see cref="LoadFailed"/> lets the hearts stay clickable, and the next click (or the next component to ask)
/// tries again.
///
/// Writes: the write repository is one scoped context for the whole circuit, so two commands must never
/// overlap. Every write runs inside the circuit-wide <see cref="CircuitWriteGate"/> (shared with the cart
/// buttons and the guest-cart merge), so hearts clicked back to back, or beside an add-to-cart on another
/// card, run one after the other. <see cref="CartState.EnsureInitializedAsync"/> is awaited BEFORE entering
/// the gate (the merge it runs takes the gate itself; awaiting it inside would deadlock).
/// </summary>
public sealed class WishlistState(ISender sender, CartState cartState, CircuitWriteGate writeGate)
{
    private const string GenericError = "Không thể cập nhật danh sách yêu thích. Vui lòng thử lại.";
    private const string LoadError = "Không thể tải danh sách yêu thích. Vui lòng thử lại.";

    private HashSet<int> _ids = [];
    private Task? _loading;
    private string? _userId;
    private string? _anonymousId;

    /// <summary>True once the first load has finished, for an anonymous visitor as well.</summary>
    public bool IsLoaded { get; private set; }

    /// <summary>True when the last load failed (hearts then stay enabled and a click retries the load).</summary>
    public bool LoadFailed { get; private set; }

    /// <summary>True when a heart may be clicked: the state is loaded, or loading failed and a click retries.</summary>
    public bool IsReady => IsLoaded || LoadFailed;

    /// <summary>True for a signed-in customer (a heart then saves; otherwise it asks them to sign in).</summary>
    public bool IsSignedIn => !string.IsNullOrWhiteSpace(_userId);

    public event Action? OnChange;

    public bool Contains(int productId) => _ids.Contains(productId);

    /// <summary>
    /// Loads the saved ids once per circuit; concurrent callers await the same Task. A load that failed is
    /// retried by the next caller. The identifiers are the ones <c>CartInitializer</c> uses, so the cart
    /// initialisation this state waits for before a write is the very same one.
    /// </summary>
    public Task EnsureLoadedAsync(string? userId, string? anonymousId)
    {
        if (IsLoaded)
            return Task.CompletedTask;

        // Still running: share it. Finished without success: start again. (Assigning after the call, never
        // from inside it, so a load that fails synchronously cannot be overwritten by its own task.)
        if (_loading is { IsCompleted: false } running)
            return running;

        return _loading = LoadCoreAsync(userId, anonymousId);
    }

    private async Task LoadCoreAsync(string? userId, string? anonymousId)
    {
        _userId = string.IsNullOrWhiteSpace(userId) ? null : userId;
        _anonymousId = anonymousId;
        LoadFailed = false;

        try
        {
            if (IsSignedIn)
                _ids = (await sender.Send(new GetMyWishlistProductIdsQuery())).ToHashSet();

            IsLoaded = true;
        }
        catch
        {
            LoadFailed = true;
        }

        OnChange?.Invoke();
    }

    /// <summary>
    /// Saves the product if it is not saved, removes it if it is. The decision is taken inside the write
    /// gate, so it always reflects the previous click. Never throws for an expected failure.
    /// </summary>
    public async Task<WishlistToggleResult> ToggleAsync(int productId, CancellationToken cancellationToken = default)
    {
        if (!IsLoaded)
        {
            // The load failed earlier: try once more, on this click.
            await EnsureLoadedAsync(_userId, _anonymousId);
            if (!IsLoaded)
                return new(WishlistOutcome.Failed, LoadError);
        }

        if (!IsSignedIn)
            return new(WishlistOutcome.RequiresSignIn);

        try
        {
            // Before the gate, not inside it: the guest-cart merge it waits for takes the gate itself.
            // CartState swallows its own failures, so this never throws.
            await cartState.EnsureInitializedAsync(_userId, _anonymousId);

            return await writeGate.RunAsync(async () =>
            {
                var add = !_ids.Contains(productId);
                var result = add
                    ? await sender.Send(new AddToWishlistCommand(productId), cancellationToken)
                    : await sender.Send(new RemoveFromWishlistCommand(productId), cancellationToken);

                if (!result.IsSuccess)
                {
                    var message = result.ValidationErrors.Select(e => e.ErrorMessage)
                        .Concat(result.Errors)
                        .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
                    return new WishlistToggleResult(WishlistOutcome.Failed, message ?? GenericError);
                }

                // Copy-on-write: a component enumerating the old set never sees it change underneath it.
                var next = new HashSet<int>(_ids);
                if (add) next.Add(productId); else next.Remove(productId);
                _ids = next;

                OnChange?.Invoke();
                return new WishlistToggleResult(WishlistOutcome.Done);
            }, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // The outcome is unknown (a save may or may not have landed): take the truth from the
            // database (a read, fresh context) so the heart shows what is really saved.
            await TryRefreshAsync();
            return new(WishlistOutcome.Failed, GenericError);
        }
    }

    private async Task TryRefreshAsync()
    {
        try
        {
            _ids = (await sender.Send(new GetMyWishlistProductIdsQuery())).ToHashSet();
        }
        catch
        {
            // Keep the last known set.
        }

        OnChange?.Invoke();
    }
}
