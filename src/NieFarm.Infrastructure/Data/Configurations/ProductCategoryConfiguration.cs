using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.HasKey(c => c.Id);

        // Accent-insensitive so the shop search can match the category name too (R-13); see
        // ProductConfiguration for why Latin1_General_100_CI_AI.
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150)
            .UseCollation("Latin1_General_100_CI_AI");
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(160);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => new { c.SortOrder, c.Id });
    }
}
