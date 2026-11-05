using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// No navigation property from CropProduct to Product (HasOne&lt;Product&gt;() with no
/// navigation) — the crop aggregate is queried standalone, exactly as CartItem -> Product is
/// one-directional. Cascade is required, not optional: DeleteProductCommandHandler deletes
/// products with no FK guard, so Restrict would turn an existing admin action into a runtime
/// failure — the same reasoning as CartItemConfiguration and ReviewConfiguration. Crop ->
/// CropProducts is also Cascade (CropConfiguration); the two roots are independent, so SQL Server
/// has no multiple-cascade-path problem (CartItems already has the same shape).
/// </summary>
public class CropProductConfiguration : IEntityTypeConfiguration<CropProduct>
{
    public void Configure(EntityTypeBuilder<CropProduct> builder)
    {
        builder.ToTable("CropProducts");
        builder.HasKey(p => p.Id);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.CropId, p.ProductId }).IsUnique();
        builder.HasIndex(p => new { p.CropId, p.SortOrder });
    }
}
