using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZones");
        builder.HasKey(z => z.Id);

        builder.Property(z => z.Name).IsRequired().HasMaxLength(ShippingZone.NameMaxLength);
        builder.HasIndex(z => z.Name).IsUnique();
        builder.Property(z => z.Fee).HasPrecision(18, 2);

        builder.HasMany(z => z.Provinces)
            .WithOne()
            .HasForeignKey(p => p.ShippingZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(ShippingZone.Provinces))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
