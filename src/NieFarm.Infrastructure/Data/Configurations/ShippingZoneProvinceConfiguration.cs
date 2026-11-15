using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ShippingZoneProvinceConfiguration : IEntityTypeConfiguration<ShippingZoneProvince>
{
    public void Configure(EntityTypeBuilder<ShippingZoneProvince> builder)
    {
        builder.ToTable("ShippingZoneProvinces");
        builder.HasKey(p => p.Id);

        // Global: a province belongs to at most one zone, active or not.
        builder.HasIndex(p => p.ProvinceCode).IsUnique();
    }
}
