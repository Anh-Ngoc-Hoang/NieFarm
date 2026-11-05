using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// Exactly one row per <see cref="Domain.Enums.CoffeeRegionKey"/>. The unique index and the
/// check constraint are the database half of that guarantee; the other half is that the seeder
/// creates any missing key and there is no create or delete command
/// (docs/plans/home-vietnam-coffee-stories.md D7).
/// </summary>
public class CoffeeRegionConfiguration : IEntityTypeConfiguration<CoffeeRegion>
{
    public void Configure(EntityTypeBuilder<CoffeeRegion> builder)
    {
        builder.ToTable("CoffeeRegions", t =>
            t.HasCheckConstraint("CK_CoffeeRegions_Key", "[Key] IN (1, 2, 3)"));
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Key).HasConversion<int>();
        builder.HasIndex(r => r.Key).IsUnique();

        builder.Property(r => r.Title).IsRequired().HasMaxLength(CoffeeRegion.TitleMaxLength);
        builder.Property(r => r.Tagline).HasMaxLength(CoffeeRegion.TaglineMaxLength);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(CoffeeRegion.DescriptionMaxLength);
        builder.Property(r => r.Altitude).HasMaxLength(CoffeeRegion.AltitudeMaxLength);
        builder.Property(r => r.Varieties).HasMaxLength(CoffeeRegion.VarietiesMaxLength);
        builder.Property(r => r.Climate).HasMaxLength(CoffeeRegion.ClimateMaxLength);
        builder.Property(r => r.FlavorProfile).HasMaxLength(CoffeeRegion.FlavorProfileMaxLength);
        builder.Property(r => r.ImageUrl).HasMaxLength(CoffeeRegion.ImageUrlMaxLength);
        builder.Property(r => r.ImageAlt).HasMaxLength(CoffeeRegion.ImageAltMaxLength);

        builder.HasMany(r => r.Areas)
            .WithOne()
            .HasForeignKey(a => a.CoffeeRegionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(CoffeeRegion.Areas))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
