using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class RoastBatchBrewGuideConfiguration : IEntityTypeConfiguration<RoastBatchBrewGuide>
{
    public void Configure(EntityTypeBuilder<RoastBatchBrewGuide> builder)
    {
        builder.ToTable("RoastBatchBrewGuides");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Method).HasConversion<int>();
        builder.Property(g => g.Ratio).HasMaxLength(RoastBatchBrewGuide.RatioMaxLength);
        builder.Property(g => g.BrewTime).HasMaxLength(RoastBatchBrewGuide.BrewTimeMaxLength);
        builder.Property(g => g.Note).HasMaxLength(RoastBatchBrewGuide.NoteMaxLength);

        // One guide per method per batch (RoastBatch.SetBrewGuides diffs by method).
        builder.HasIndex(g => new { g.RoastBatchId, g.Method }).IsUnique();
    }
}
