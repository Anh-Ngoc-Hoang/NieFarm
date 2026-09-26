using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// The unique index below is case-insensitive under SQL Server's default collation, while the
/// aggregate (NieFarm.Domain.Entities.Cart) compares VariantKey ordinally — safe only because
/// every caller passes labels canonicalised out of the product's own option values (CartLineKey,
/// ProductVariantResolver.Canonicalise), never raw browser input. (CartId, ProductId, VariantKey)
/// at nvarchar(400) is ~816 bytes, well inside SQL Server's 1700-byte nonclustered-index key
/// limit.
///
/// No navigation property from CartItem to Product (HasOne&lt;Product&gt;() with no navigation)
/// — the cart aggregate is queried standalone, exactly as Review -> Product is one-directional
/// and Product has no Reviews collection. Cascade is required, not optional:
/// DeleteProductCommandHandler deletes products with no FK guard, so Restrict would turn an
/// existing admin action into a runtime failure — the same reasoning as ReviewConfiguration.
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.VariantKey).IsRequired().HasMaxLength(400);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.CartId, i.ProductId, i.VariantKey }).IsUnique();
    }
}
