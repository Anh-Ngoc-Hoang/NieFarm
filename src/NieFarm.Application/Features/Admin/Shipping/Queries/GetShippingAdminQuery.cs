using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Shipping.Dtos;
using NieFarm.Application.Features.Shipping;
using NieFarm.Application.Features.Shipping.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Shipping.Queries;

public record GetShippingAdminQuery : IRequest<ShippingAdminDto>;

public class GetShippingAdminQueryHandler(
    IReadRepositoryBase<ShippingSettings> settingsRepository,
    IReadRepositoryBase<ShippingZone> zoneRepository)
    : IRequestHandler<GetShippingAdminQuery, ShippingAdminDto>
{
    public async Task<ShippingAdminDto> Handle(GetShippingAdminQuery request, CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.FirstOrDefaultAsync(new ShippingSettingsSingletonSpec(), cancellationToken);
        var zones = await zoneRepository.ListAsync(new ShippingZonesSpec(activeOnly: false), cancellationToken);

        var zoneDtos = zones
            .Select(z =>
            {
                var codes = z.Provinces.Select(p => p.ProvinceCode).OrderBy(c => c).ToList();
                return new ShippingZoneAdminDto(
                    z.Id, z.Name, z.Fee, z.SortOrder, z.IsActive, codes,
                    codes.Select(VietnamProvinceCatalog.DisplayName).OrderBy(n => n, StringComparer.CurrentCulture).ToList());
            })
            .ToList();

        var zoneByCode = new Dictionary<int, ShippingZone>();
        foreach (var zone in zones)
            foreach (var province in zone.Provinces)
                zoneByCode[province.ProvinceCode] = zone;

        var provinces = VietnamProvinceCatalog.All
            .Select(p => new ShippingProvinceAdminDto(
                p.Code, p.Name,
                zoneByCode.GetValueOrDefault(p.Code)?.Id,
                zoneByCode.GetValueOrDefault(p.Code)?.Name))
            .OrderBy(p => p.Name, StringComparer.CurrentCulture)
            .ToList();

        return new ShippingAdminDto(
            settings?.BaseFee ?? ShippingRates.FallbackBaseFee,
            settings?.FreeShippingThreshold,
            settings is null,
            zoneDtos,
            provinces);
    }
}
