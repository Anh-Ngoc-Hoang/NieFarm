using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ProductFeatureConfiguration : IEntityTypeConfiguration<ProductFeature>
{
    public void Configure(EntityTypeBuilder<ProductFeature> builder)
    {
        builder.ToTable("ProductFeatures");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Text).IsRequired().HasMaxLength(500);

        builder.HasIndex(f => new { f.ProductId, f.SortOrder });
    }
}
