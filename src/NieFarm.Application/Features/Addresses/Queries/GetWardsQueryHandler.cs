using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Addresses.Dtos;

namespace NieFarm.Application.Features.Addresses.Queries;

public class GetWardsQueryHandler(IAddressLookupService lookup)
    : IRequestHandler<GetWardsQuery, Result<IReadOnlyList<WardDto>>>
{
    public Task<Result<IReadOnlyList<WardDto>>> Handle(
        GetWardsQuery request, CancellationToken cancellationToken) =>
        lookup.GetWardsAsync(request.ProvinceCode, cancellationToken);
}
