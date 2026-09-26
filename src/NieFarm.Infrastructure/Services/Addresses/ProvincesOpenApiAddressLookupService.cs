using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Addresses.Dtos;

namespace NieFarm.Infrastructure.Services.Addresses;

/// <summary>
/// Vietnamese province/ward reference data from provinces.open-api.vn, cached and served with a
/// last-known-good fallback so a slow or down third party never breaks checkout. Registered as a
/// singleton so the de-duplication gate and stale snapshots are shared process-wide — must never
/// gain a scoped dependency (see .claude/rules for the reasoning).
/// </summary>
public class ProvincesOpenApiAddressLookupService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<AddressLookupOptions> options,
    ILogger<ProvincesOpenApiAddressLookupService> logger) : IAddressLookupService
{
    private const string ProvincesCacheKey = "address:provinces";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ConcurrentDictionary<int, IReadOnlyList<WardDto>> _lastGoodWards = new();
    private IReadOnlyList<ProvinceDto>? _lastGoodProvinces;

    public Task<Result<IReadOnlyList<ProvinceDto>>> GetProvincesAsync(CancellationToken cancellationToken) =>
        GetOrFetchAsync(ProvincesCacheKey, FetchProvincesAsync, cancellationToken);

    public Task<Result<IReadOnlyList<WardDto>>> GetWardsAsync(int provinceCode, CancellationToken cancellationToken) =>
        GetOrFetchAsync(
            WardsCacheKey(provinceCode),
            (client, ct) => FetchWardsAsync(provinceCode, client, ct),
            cancellationToken);

    private static string WardsCacheKey(int provinceCode) => $"address:wards:{provinceCode}";

    /// <summary>
    /// Cache-aside with a gate that collapses concurrent misses (prerender + circuit
    /// double-init, or several visitors hitting a cold cache at once) into one outbound
    /// request. The fetch delegate reports whether its success is a stale last-known-good
    /// value so it can be re-cached under the short failure TTL instead of the 24h one.
    /// </summary>
    private async Task<Result<T>> GetOrFetchAsync<T>(
        string cacheKey,
        Func<HttpClient, CancellationToken, Task<(Result<T> Result, bool IsStale)>> fetch,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(cacheKey, out Result<T>? hit) && hit is not null)
            return hit;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(cacheKey, out hit) && hit is not null)
                return hit;

            var client = httpClientFactory.CreateClient(AddressLookupOptions.SectionName);
            var (result, isStale) = await fetch(client, cancellationToken);

            var ttl = result.IsSuccess && !isStale
                ? TimeSpan.FromHours(options.Value.CacheHours)
                : TimeSpan.FromSeconds(options.Value.FailureCacheSeconds);

            cache.Set(cacheKey, result, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<(Result<IReadOnlyList<ProvinceDto>> Result, bool IsStale)> FetchProvincesAsync(
        HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetFromJsonAsync<List<ProvinceResponse>>("p/", JsonOptions, cancellationToken);
            var provinces = (response ?? [])
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .Select(p => new ProvinceDto(p.Code, p.Name!))
                .ToList();

            _lastGoodProvinces = provinces;
            return (Result<IReadOnlyList<ProvinceDto>>.Success(provinces), false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // genuine caller cancellation (circuit teardown) — do not swallow
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Address lookup failed for {CacheKey}", ProvincesCacheKey);

            return _lastGoodProvinces is not null
                ? (Result<IReadOnlyList<ProvinceDto>>.Success(_lastGoodProvinces), true) // serve stale rather than nothing
                : (Result<IReadOnlyList<ProvinceDto>>.Unavailable("Không tải được danh sách địa chỉ."), false);
        }
    }

    private async Task<(Result<IReadOnlyList<WardDto>> Result, bool IsStale)> FetchWardsAsync(
        int provinceCode, HttpClient client, CancellationToken cancellationToken)
    {
        var cacheKey = WardsCacheKey(provinceCode);

        try
        {
            var response = await client.GetAsync($"p/{provinceCode}?depth=2", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return (Result<IReadOnlyList<WardDto>>.NotFound(), false);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<ProvinceWithWardsResponse>(
                JsonOptions, cancellationToken);
            var wards = (payload?.Wards ?? [])
                .Where(w => !string.IsNullOrWhiteSpace(w.Name))
                .Select(w => new WardDto(w.Code, w.Name!))
                .ToList();

            _lastGoodWards[provinceCode] = wards;
            return (Result<IReadOnlyList<WardDto>>.Success(wards), false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // genuine caller cancellation (circuit teardown) — do not swallow
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Address lookup failed for {CacheKey}", cacheKey);

            return _lastGoodWards.TryGetValue(provinceCode, out var lastGood)
                ? (Result<IReadOnlyList<WardDto>>.Success(lastGood), true) // serve stale rather than nothing
                : (Result<IReadOnlyList<WardDto>>.Unavailable("Không tải được danh sách địa chỉ."), false);
        }
    }
}
