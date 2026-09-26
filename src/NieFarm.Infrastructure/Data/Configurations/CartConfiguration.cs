using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(c => c.Id);

        // Match AspNetUsers.Id (nvarchar(450)), as OrderConfiguration does.
        builder.Property(c => c.UserId).HasMaxLength(450);
        builder.Property(c => c.AnonymousId).HasMaxLength(64);

        // A cart row is owned by a user or by an anonymous id, never both, and each owner has
        // at most one.
        builder.HasIndex(c => c.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
        builder.HasIndex(c => c.AnonymousId).IsUnique().HasFilter("[AnonymousId] IS NOT NULL");

        // Unlike Orders (SetNull — an order must outlive its account), a cart is transient:
        // deleting the account deletes the cart. Single cascade path, no conflict.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // The aggregate exposes its lines read-only, so EF reads and writes the backing field.
        builder.Metadata.FindNavigation(nameof(Cart.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
