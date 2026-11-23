namespace NieFarm.Web.Services;

/// <summary>
/// Scoped (per-circuit) gate that lets only one write command run at a time on the circuit's shared write
/// context (data-access.md: <c>IRepositoryBase&lt;T&gt;</c> is one scoped <c>AppDbContext</c> for the whole
/// Blazor circuit). Two commands overlapping on it throw "A second operation was started" before
/// <c>SaveChanges</c>, and the entity added/removed by the failed one stays in the change tracker, so every later
/// <c>DiscardTrackedEntities()</c> in that circuit throws "unsaved changes" until the page is reloaded.
///
/// Used by every component that can write while another can: the wishlist hearts, the cart buttons on
/// <c>ProductCard</c> and the PDP, the PDP review form, and the guest-cart merge in <c>CartState</c>.
/// Checkout and the account pages keep their own <c>_updating</c>/<c>_busy</c> guards (they never share a page
/// with those components). The delegate is the whole critical section: do not await anything that itself needs
/// the gate inside it (the gate is not re-entrant), and await <c>CartState.EnsureInitializedAsync</c> BEFORE
/// entering, since that initialisation takes the gate too.
/// </summary>
public sealed class CircuitWriteGate
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await action();
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task RunAsync(Func<Task> action, CancellationToken cancellationToken = default)
        => RunAsync(async () => { await action(); return true; }, cancellationToken);
}
