using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Shipping.Dtos;

namespace NieFarm.Application.Features.Shipping.Queries;

public record GetShippingPolicyQuery : IRequest<ShippingPolicyDto>;

public class GetShippingPolicyQueryHandler(IShippingRateProvider rateProvider)
    : IRequestHandler<GetShippingPolicyQuery, ShippingPolicyDto>
{
    public async Task<ShippingPolicyDto> Handle(GetShippingPolicyQuery request, CancellationToken cancellationToken)
    {
        var rates = await rateProvider.GetRatesAsync(cancellationToken);

        var zones = rates.Zones
            .Select(z => new ShippingPolicyZoneDto(
                z.Name,
                z.Fee,
                z.ProvinceCodes.Select(VietnamProvinceCatalog.DisplayName)
                    .OrderBy(n => n, StringComparer.CurrentCulture).ToList()))
            .ToList();

        var fees = zones.Select(z => z.Fee).Append(rates.BaseFee).ToList();
        return new ShippingPolicyDto(rates.BaseFee, rates.FreeShippingThreshold, zones, fees.Min(), fees.Max());
    }
}
