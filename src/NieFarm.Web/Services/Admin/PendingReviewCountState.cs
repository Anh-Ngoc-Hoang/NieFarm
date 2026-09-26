using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using NieFarm.Application.Features.Admin.Reviews.Queries;

namespace NieFarm.Web.Services.Admin;

/// <summary>
/// Scoped (per-circuit) count of pending reviews, backing the sidebar badge. Mirrors
/// <see cref="NewOrderCountState"/> exactly — re-queries on every navigation so moderating a
/// review updates the badge without a manual refresh.
/// </summary>
public sealed class PendingReviewCountState : IDisposable
{
    private readonly ISender _sender;
    private readonly NavigationManager _navigation;

    public PendingReviewCountState(ISender sender, NavigationManager navigation)
    {
        _sender = sender;
        _navigation = navigation;
        _navigation.LocationChanged += HandleLocationChanged;
    }

    public int Count { get; private set; }

    public event Action? OnChange;

    public async Task RefreshAsync()
    {
        int count;
        try
        {
            count = await _sender.Send(new GetPendingReviewCountQuery());
        }
        catch
        {
            // A badge is never worth breaking the admin shell over — most likely the
            // database is not migrated yet.
            count = 0;
        }

        if (count == Count)
            return;

        Count = count;
        OnChange?.Invoke();
    }

    private async void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        try
        {
            await RefreshAsync();
        }
        catch
        {
            // Fired from an event handler with no caller to observe a failure.
        }
    }

    public void Dispose() => _navigation.LocationChanged -= HandleLocationChanged;
}
