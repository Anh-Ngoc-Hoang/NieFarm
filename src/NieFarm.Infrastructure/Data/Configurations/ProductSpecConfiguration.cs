using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ProductSpecConfiguration : IEntityTypeConfiguration<ProductSpec>
{
    public void Configure(EntityTypeBuilder<ProductSpec> builder)
    {
        builder.ToTable("ProductSpecs");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Label).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Value).IsRequired().HasMaxLength(300);

        builder.HasIndex(s => new { s.ProductId, s.SortOrder });
    }
}
