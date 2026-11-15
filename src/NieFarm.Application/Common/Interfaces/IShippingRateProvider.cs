using NieFarm.Application.Features.Shipping;

namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// The one source of shipping fees. Implemented in Infrastructure by reading the admin-managed
/// settings and zones fresh on every call (no cache), so an admin save is visible on the next cart
/// build. Never throws for bad stored data: it falls back to a safe default instead.
/// </summary>
public interface IShippingRateProvider
{
    Task<ShippingRates> GetRatesAsync(CancellationToken cancellationToken);
}
