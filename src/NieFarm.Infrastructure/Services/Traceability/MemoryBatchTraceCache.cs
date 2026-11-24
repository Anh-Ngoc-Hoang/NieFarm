using Microsoft.Extensions.Caching.Memory;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Traceability.Dtos;

namespace NieFarm.Infrastructure.Services.Traceability;

/// <summary>
/// 60-second in-memory cache of the public page data of published batches (R-26 D15). It shares the app-wide
/// <see cref="IMemoryCache"/>, which has no size limit, so entries carry no Size; that is safe because the key space
/// is bounded by the number of published batches (the handler never stores a miss or an admin preview).
/// </summary>
internal sealed class MemoryBatchTraceCache(IMemoryCache cache) : IBatchTraceCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

    public bool TryGet(string code, out BatchTraceDto? dto)
    {
        if (cache.TryGetValue(Key(code), out BatchTraceDto? hit) && hit is not null)
        {
            dto = hit;
            return true;
        }

        dto = null;
        return false;
    }

    public void Set(string code, BatchTraceDto dto) => cache.Set(Key(code), dto, Ttl);

    public void Invalidate(string code) => cache.Remove(Key(code));

    private static string Key(string code) => $"trace:{code}";
}
