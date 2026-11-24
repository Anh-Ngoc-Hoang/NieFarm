using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using NieFarm.Application.Features.Traceability.Commands;
using NieFarm.Web.Content;
using NieFarm.Web.Services.Http;

namespace NieFarm.Web.Services.Traceability;

/// <summary>
/// Counts scans in memory, with no I/O on the request (R-26 D10/D11): <see cref="TryRecord"/> only bumps a counter,
/// and <see cref="BatchScanFlushService"/> drains the counters on a timer into one small database write. A client
/// (address + user agent, hashed with a key made at process start and kept only in memory) counts once per batch per
/// <c>ScanDedupeMinutes</c>; the dedupe cache has a size limit, and when it is full a new entry is simply not stored,
/// so a flood is over-counted, which is itself the signal. Nothing identifying is ever stored, logged or returned.
/// A crash loses at most one flush interval of counts; the design assumes one process.
/// </summary>
public sealed class BatchScanRecorder(TraceabilitySettingsProvider settings) : IDisposable
{
    private const int DedupeSizeLimit = 100_000;

    private sealed class Tally
    {
        public int Count;
        public DateTime FirstUtc;
        public DateTime LastUtc;
        public bool Closed;
    }

    private readonly byte[] _clientKey = RandomNumberGenerator.GetBytes(32);
    private readonly MemoryCache _dedupe = new(new MemoryCacheOptions { SizeLimit = DedupeSizeLimit });
    private ConcurrentDictionary<(int BatchId, DateOnly Day), Tally> _tallies = new();

    /// <summary>
    /// Records one scan unless it is not a person (not GET, prefetch, bot) or a repeat within the dedupe window.
    /// The caller has already established the batch exists, is published and the viewer is not an admin.
    /// </summary>
    public bool TryRecord(HttpContext context, int batchId)
    {
        var request = context.Request;
        if (!HttpMethods.IsGet(request.Method) || ScanClientClassifier.IsPrefetch(request))
            return false;

        var userAgent = request.Headers.UserAgent.ToString();
        if (ScanClientClassifier.IsLikelyBot(userAgent))
            return false;

        var window = settings.Current.ScanDedupeMinutes;
        if (window > 0)
        {
            var key = $"{batchId}:{ClientId(ClientIpKey.For(context), userAgent)}";
            if (_dedupe.TryGetValue(key, out _))
                return false;

            _dedupe.Set(key, true, new MemoryCacheEntryOptions
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(window)
            });
        }

        var now = DateTime.UtcNow;
        Add(batchId, DateOnly.FromDateTime(VnTime.FromUtc(now)), 1, now, now);
        return true;
    }

    /// <summary>Takes every counter out of memory. Anything counted after the swap lands in the next drain.</summary>
    public List<BatchScanTally> Drain()
    {
        var old = Interlocked.Exchange(ref _tallies, new ConcurrentDictionary<(int BatchId, DateOnly Day), Tally>());

        var result = new List<BatchScanTally>(old.Count);
        foreach (var (key, tally) in old)
        {
            lock (tally)
            {
                // A writer that fetched this tally just before the swap sees Closed and retries on the new map.
                tally.Closed = true;
                if (tally.Count > 0)
                    result.Add(new BatchScanTally(key.BatchId, key.Day, tally.Count, tally.FirstUtc, tally.LastUtc));
            }
        }

        return result;
    }

    /// <summary>Puts tallies back after a failed flush, merging with what has been counted since.</summary>
    public void Requeue(IEnumerable<BatchScanTally> tallies)
    {
        foreach (var t in tallies)
            Add(t.BatchId, t.DayVn, t.Count, t.FirstAtUtc, t.LastAtUtc);
    }

    private void Add(int batchId, DateOnly day, int count, DateTime firstUtc, DateTime lastUtc)
    {
        while (true)
        {
            var tally = _tallies.GetOrAdd((batchId, day), _ => new Tally { FirstUtc = firstUtc, LastUtc = lastUtc });
            lock (tally)
            {
                if (tally.Closed)
                    continue;

                tally.Count = (int)Math.Min((long)tally.Count + count, int.MaxValue);
                if (firstUtc < tally.FirstUtc)
                    tally.FirstUtc = firstUtc;
                if (lastUtc > tally.LastUtc)
                    tally.LastUtc = lastUtc;
                return;
            }
        }
    }

    private string ClientId(string ipKey, string userAgent)
    {
        var hash = HMACSHA256.HashData(_clientKey, Encoding.UTF8.GetBytes(ipKey + "\n" + userAgent));
        return Convert.ToHexString(hash.AsSpan(0, 16));
    }

    public void Dispose() => _dedupe.Dispose();
}
