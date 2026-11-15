using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Shipping;
using NieFarm.Domain.Entities;
using NieFarm.Infrastructure.Data;

namespace NieFarm.Infrastructure.Services.Shipping;

/// <summary>
/// Reads the admin-managed shipping settings and active zones fresh on every call: this provider is
/// scoped (one per circuit), so it opens a new short-lived context per call and queries with
/// AsNoTracking. Holding one context would serve stale rates for the whole circuit and could throw on
/// overlapping calls. There is deliberately no cache: the data is tiny and an admin save must show up
/// on the next cart build. Fail-safe: a missing row, an invalid
/// stored value or a failing read returns a 30.000 base fee with no free-shipping threshold, and an
/// invalid zone is skipped. Errors are logged at most once per five minutes per kind so a broken row
/// cannot flood the log on every cart build.
/// </summary>
public class DbShippingRateProvider(
    IDbContextFactory<AppDbContext> contextFactory,
    ILogger<DbShippingRateProvider> logger) : IShippingRateProvider
{
    private static readonly TimeSpan LogInterval = TimeSpan.FromMinutes(5);
    private static readonly object LogLock = new();
    private static readonly Dictionary<string, DateTime> LastLogged = [];

    public async Task<ShippingRates> GetRatesAsync(CancellationToken cancellationToken)
    {
        ShippingSettings? settings;
        List<ShippingZone> zones;

        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            settings = await db.ShippingSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SingletonKey == ShippingSettings.SingletonValue, cancellationToken);
            zones = await db.ShippingZones.AsNoTracking()
                .Include(z => z.Provinces)
                .Where(z => z.IsActive)
                .OrderBy(z => z.SortOrder).ThenBy(z => z.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogThrottled("read", ex, "Reading the shipping settings failed; using the fallback fee of {Fee}.");
            return ShippingRates.Fallback;
        }

        if (settings is null)
        {
            LogThrottled("missing", null,
                "The ShippingSettings row is missing; using the fallback fee of {Fee} and no free-shipping threshold. It is re-created on the next start.");
            return ShippingRates.Fallback;
        }

        if (!ShippingSettings.IsValidFee(settings.BaseFee)
            || settings.FreeShippingThreshold is { } t && (t <= 0 || t != decimal.Truncate(t)))
        {
            LogThrottled("invalid", null,
                "The ShippingSettings row holds invalid values; using the fallback fee of {Fee} and no free-shipping threshold.");
            return ShippingRates.Fallback;
        }

        var rates = new List<ShippingZoneRate>();
        foreach (var zone in zones)
        {
            if (!ShippingSettings.IsValidFee(zone.Fee))
            {
                LogThrottled("zone-" + zone.Id, null,
                    "A shipping zone has an invalid fee and is skipped (its provinces pay the base fee); fallback fee {Fee}.");
                continue;
            }

            rates.Add(new ShippingZoneRate(
                zone.Id, zone.Name, zone.Fee, zone.Provinces.Select(p => p.ProvinceCode).ToHashSet()));
        }

        return new ShippingRates(settings.BaseFee, settings.FreeShippingThreshold, rates);
    }

    private void LogThrottled(string kind, Exception? ex, string message)
    {
        lock (LogLock)
        {
            var now = DateTime.UtcNow;
            if (LastLogged.TryGetValue(kind, out var last) && now - last < LogInterval)
                return;
            LastLogged[kind] = now;
        }

        if (ex is null)
            logger.LogError(message, ShippingRates.FallbackBaseFee);
        else
            logger.LogError(ex, message, ShippingRates.FallbackBaseFee);
    }
}
