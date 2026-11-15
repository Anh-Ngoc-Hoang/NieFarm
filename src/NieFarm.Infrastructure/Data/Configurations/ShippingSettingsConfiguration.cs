using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// Exactly one row: SingletonKey is always 1, enforced by a unique index and a check constraint
/// (the CoffeeRegions pattern); the seeder inserts a missing row.
/// </summary>
public class ShippingSettingsConfiguration : IEntityTypeConfiguration<ShippingSettings>
{
    public void Configure(EntityTypeBuilder<ShippingSettings> builder)
    {
        builder.ToTable("ShippingSettings", t =>
            t.HasCheckConstraint("CK_ShippingSettings_Singleton", "[SingletonKey] = 1"));
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.SingletonKey).IsUnique();
        builder.Property(s => s.BaseFee).HasPrecision(18, 2);
        builder.Property(s => s.FreeShippingThreshold).HasPrecision(18, 2);
    }
}
