using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// 1:1 with a batch by BatchId, with no navigation either way, so saving a batch (a whole-row update) can never
/// overwrite the counters (R-26 D12). Deleting a batch deletes its counters.
/// </summary>
public class RoastBatchScanStatConfiguration : IEntityTypeConfiguration<RoastBatchScanStat>
{
    public void Configure(EntityTypeBuilder<RoastBatchScanStat> builder)
    {
        builder.ToTable("RoastBatchScanStats");
        builder.HasKey(s => s.Id);

        builder.HasOne<RoastBatch>()
            .WithMany()
            .HasForeignKey(s => s.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.BatchId).IsUnique().HasDatabaseName("IX_RoastBatchScanStats_BatchId");

        // Sidebar badge: count the batches with an open alert.
        builder.HasIndex(s => s.AlertRaisedAt);
    }
}
