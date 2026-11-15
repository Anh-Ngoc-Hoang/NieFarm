using NieFarm.Application.Features.Orders.Dtos;

namespace NieFarm.Web.Services.Orders;

/// <summary>
/// Circuit-scoped hand-off of the just-placed order from Checkout to the success page. Checkout
/// navigates in-circuit, so the scope survives; a hard reload or a new tab starts a new circuit and
/// the state is gone. The success page reads only this and never queries by code: order codes are
/// guessable, so loading by code alone would be an enumeration hole.
///
/// Second use: the phone the customer typed at checkout is kept alongside, so the order-lookup page
/// reached from the success page can run its normal code + phone lookup without retyping. The phone
/// lives in this object's memory only: never in a URL, query string, log or any storage outside the
/// circuit. The lookup is still verified server-side; a code in a URL alone yields nothing here.
/// </summary>
public sealed class PlacedOrderState
{
    private OrderPlacedDto? _placed;
    private string? _phone;

    public void Set(OrderPlacedDto placed, string? phone)
    {
        _placed = placed;
        _phone = phone;
    }

    /// <summary>The placed order, only when its code matches the one in the URL.</summary>
    public OrderPlacedDto? Get(string code) =>
        _placed is not null && string.Equals(_placed.Code, code, StringComparison.OrdinalIgnoreCase)
            ? _placed
            : null;

    /// <summary>
    /// The phone typed at checkout, only when <paramref name="code"/> matches the placed order
    /// (same rule as <see cref="Get"/>) and a phone was captured.
    /// </summary>
    public bool TryGetLookupCredentials(string code, out string phone)
    {
        if (Get(code) is not null && !string.IsNullOrWhiteSpace(_phone))
        {
            phone = _phone;
            return true;
        }

        phone = "";
        return false;
    }
}
