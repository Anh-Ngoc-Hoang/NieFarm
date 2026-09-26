using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ProductOptionValueConfiguration : IEntityTypeConfiguration<ProductOptionValue>
{
    public void Configure(EntityTypeBuilder<ProductOptionValue> builder)
    {
        builder.ToTable("ProductOptionValues");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Label).IsRequired().HasMaxLength(100);

        builder.HasIndex(v => new { v.ProductOptionId, v.SortOrder });
    }
}
