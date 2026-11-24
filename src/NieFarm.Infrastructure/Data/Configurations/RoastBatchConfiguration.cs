using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// Restrict on the product FK is deliberate (R-26 D7): a batch has a printed QR, so deleting its product
/// must not silently erase it. DeleteProductCommandHandler checks first and answers with a friendly error.
/// </summary>
public class RoastBatchConfiguration : IEntityTypeConfiguration<RoastBatch>
{
    public void Configure(EntityTypeBuilder<RoastBatch> builder)
    {
        builder.ToTable("RoastBatches");
        builder.HasKey(b => b.Id);

        // Fixed-width ASCII code: it is what the unique index is built on.
        builder.Property(b => b.Code).IsRequired().IsUnicode(false)
            .HasMaxLength(RoastBatch.CodeLength).IsFixedLength();
        builder.HasIndex(b => b.Code).IsUnique().HasDatabaseName("IX_RoastBatches_Code");

        builder.Property(b => b.InternalNote).HasMaxLength(RoastBatch.InternalNoteMaxLength);
        builder.Property(b => b.OriginProvince).HasMaxLength(RoastBatch.OriginProvinceMaxLength);
        builder.Property(b => b.OriginCommune).HasMaxLength(RoastBatch.OriginCommuneMaxLength);
        builder.Property(b => b.OriginLocality).HasMaxLength(RoastBatch.OriginLocalityMaxLength);
        builder.Property(b => b.Variety).HasMaxLength(RoastBatch.VarietyMaxLength);
        builder.Property(b => b.FarmName).HasMaxLength(RoastBatch.FarmNameMaxLength);
        builder.Property(b => b.FarmStory).HasMaxLength(RoastBatch.FarmStoryMaxLength);
        builder.Property(b => b.FarmImageUrl).HasMaxLength(RoastBatch.ImageUrlMaxLength);
        builder.Property(b => b.HarvestSeason).HasMaxLength(RoastBatch.HarvestSeasonMaxLength);
        builder.Property(b => b.HarvestMethod).HasMaxLength(RoastBatch.HarvestMethodMaxLength);
        builder.Property(b => b.ProcessingNote).HasMaxLength(RoastBatch.ProcessingNoteMaxLength);
        builder.Property(b => b.DryingMethod).HasMaxLength(RoastBatch.DryingMethodMaxLength);
        builder.Property(b => b.RoasterName).HasMaxLength(RoastBatch.RoasterNameMaxLength);
        builder.Property(b => b.RoastNote).HasMaxLength(RoastBatch.RoastNoteMaxLength);
        builder.Property(b => b.PackagingNote).HasMaxLength(RoastBatch.PackagingNoteMaxLength);
        builder.Property(b => b.CuppedBy).HasMaxLength(RoastBatch.CuppedByMaxLength);
        builder.Property(b => b.FlavourNotes).HasMaxLength(RoastBatch.FlavourNotesMaxLength);
        builder.Property(b => b.ScreenSize).HasMaxLength(RoastBatch.ScreenSizeMaxLength);
        builder.Property(b => b.LabReportImageUrl).HasMaxLength(RoastBatch.ImageUrlMaxLength);
        builder.Property(b => b.StorageTip).HasMaxLength(RoastBatch.StorageTipMaxLength);

        builder.Property(b => b.CuppingScore).HasPrecision(5, 2);
        builder.Property(b => b.MoisturePercent).HasPrecision(4, 1);
        builder.Property(b => b.DefectRatePercent).HasPrecision(4, 1);

        builder.HasOne(b => b.Product)
            .WithMany()
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.ProductId, b.RoastDate });

        builder.HasMany(b => b.Certifications)
            .WithOne()
            .HasForeignKey(c => c.RoastBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BrewGuides)
            .WithOne()
            .HasForeignKey(g => g.RoastBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(RoastBatch.Certifications))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(RoastBatch.BrewGuides))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
