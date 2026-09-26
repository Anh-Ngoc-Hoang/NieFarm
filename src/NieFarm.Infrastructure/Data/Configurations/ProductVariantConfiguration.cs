using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);

        // Money is decimal(18,2) throughout — see .claude/rules/conventions.md.
        builder.Property(v => v.Price).HasPrecision(18, 2);

        builder.Property(v => v.ImageUrl).HasMaxLength(500);

        builder.HasMany(v => v.Values)
            .WithOne()
            .HasForeignKey(vv => vv.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.ProductId, v.SortOrder });

        builder.Metadata.FindNavigation(nameof(ProductVariant.Values))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
