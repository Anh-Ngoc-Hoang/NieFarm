using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class RoastBatchCertificationConfiguration : IEntityTypeConfiguration<RoastBatchCertification>
{
    public void Configure(EntityTypeBuilder<RoastBatchCertification> builder)
    {
        builder.ToTable("RoastBatchCertifications");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(RoastBatchCertification.NameMaxLength);
        builder.Property(c => c.Number).HasMaxLength(RoastBatchCertification.NumberMaxLength);

        builder.HasIndex(c => new { c.RoastBatchId, c.SortOrder });
    }
}
