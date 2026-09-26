using Ardalis.Result;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Addresses.Dtos;

namespace NieFarm.Application.Features.Addresses.Queries;

public class GetProvincesQueryHandler(IAddressLookupService lookup)
    : IRequestHandler<GetProvincesQuery, Result<IReadOnlyList<ProvinceDto>>>
{
    public Task<Result<IReadOnlyList<ProvinceDto>>> Handle(
        GetProvincesQuery request, CancellationToken cancellationToken) =>
        lookup.GetProvincesAsync(cancellationToken);
}
