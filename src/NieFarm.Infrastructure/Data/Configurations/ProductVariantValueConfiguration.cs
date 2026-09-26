using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ProductVariantValueConfiguration : IEntityTypeConfiguration<ProductVariantValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantValue> builder)
    {
        builder.ToTable("ProductVariantValues");
        builder.HasKey(vv => vv.Id);

        // ClientCascade, not Cascade: two paths reach this table from Product
        // (Product → Variant → VariantValue and Product → Option → OptionValue → VariantValue),
        // and a second database-level cascade would make SQL Server reject the migration with
        // "may cause cycles or multiple cascade paths" (FK 1785). EF deletes these rows in
        // memory instead, which is why every write path loads them — see ProductByIdWithDetailsSpec.
        builder.HasOne(vv => vv.OptionValue)
            .WithMany()
            .HasForeignKey(vv => vv.ProductOptionValueId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasIndex(vv => new { vv.ProductVariantId, vv.ProductOptionValueId }).IsUnique();
    }
}
