using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class CropConfiguration : IEntityTypeConfiguration<Crop>
{
    public void Configure(EntityTypeBuilder<Crop> builder)
    {
        builder.ToTable("Crops");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.SeasonLabel).IsRequired().HasMaxLength(Crop.SeasonLabelMaxLength);
        builder.Property(c => c.Headline).IsRequired().HasMaxLength(Crop.HeadlineMaxLength);
        builder.Property(c => c.Story).IsRequired().HasMaxLength(Crop.StoryMaxLength);
        builder.Property(c => c.Region).HasMaxLength(Crop.RegionMaxLength);
        builder.Property(c => c.Altitude).HasMaxLength(Crop.AltitudeMaxLength);
        builder.Property(c => c.Varieties).HasMaxLength(Crop.VarietiesMaxLength);
        builder.Property(c => c.Process).HasMaxLength(Crop.ProcessMaxLength);
        builder.Property(c => c.TastingNotes).HasMaxLength(Crop.TastingNotesMaxLength);
        builder.Property(c => c.ImageUrl).IsRequired().HasMaxLength(Crop.ImageUrlMaxLength);
        builder.Property(c => c.ImageAlt).IsRequired().HasMaxLength(Crop.ImageAltMaxLength);

        builder.HasIndex(c => c.Year).IsUnique();

        // The single-current invariant's database backstop (D1): at most one row can ever have
        // IsCurrent = 1. SetCurrentCropCommandHandler clears the old current row in its own
        // SaveChanges before marking the new one, specifically so this filtered index is never
        // asked to hold two 1s at once mid-batch.
        builder.HasIndex(c => c.IsCurrent).IsUnique().HasFilter("[IsCurrent] = 1");

        builder.HasMany(c => c.Products)
            .WithOne()
            .HasForeignKey(p => p.CropId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Crop.Products))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
