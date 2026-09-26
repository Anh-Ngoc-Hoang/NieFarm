using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        builder.ToTable("ProductOptions");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(o => o.Values)
            .WithOne()
            .HasForeignKey(v => v.ProductOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.ProductId, o.SortOrder });

        // The collection is exposed read-only over a backing field.
        builder.Metadata.FindNavigation(nameof(ProductOption.Values))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
