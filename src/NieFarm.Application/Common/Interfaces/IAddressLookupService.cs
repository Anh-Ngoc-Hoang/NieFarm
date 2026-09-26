using Ardalis.Result;
using NieFarm.Application.Features.Addresses.Dtos;

namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Vietnamese administrative divisions, sourced from a third-party service and implemented in
/// Infrastructure so the Application layer never touches HTTP. Transport failures are returned
/// as Result.Unavailable, never thrown — checkout must stay usable when the source is down.
/// </summary>
public interface IAddressLookupService
{
    Task<Result<IReadOnlyList<ProvinceDto>>> GetProvincesAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<WardDto>>> GetWardsAsync(int provinceCode, CancellationToken cancellationToken);
}
