using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using NieFarm.Application.Features.Admin.Orders.Queries;

namespace NieFarm.Web.Services.Admin;

/// <summary>
/// Scoped (per-circuit) count of unread orders, backing the sidebar badge. It re-queries on
/// every navigation so acting on an order updates the badge without a manual refresh.
/// </summary>
public sealed class NewOrderCountState : IDisposable
{
    private readonly ISender _sender;
    private readonly NavigationManager _navigation;

    public NewOrderCountState(ISender sender, NavigationManager navigation)
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
            count = await _sender.Send(new GetUnreadOrderCountQuery());
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
