using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// Both foreign keys cascade: the wishlist is the customer's own data (deleting the account deletes it), and
/// DeleteProductCommandHandler deletes products with no FK guard, so Restrict would turn an existing admin
/// action into a runtime failure — the same reasoning as CartItemConfiguration and ReviewConfiguration.
/// </summary>
public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");
        builder.HasKey(w => w.Id);

        // Match AspNetUsers.Id (nvarchar(450)), as CartConfiguration does.
        builder.Property(w => w.UserId).IsRequired().HasMaxLength(WishlistItem.UserIdMaxLength);

        // One row per (user, product): the backstop for a double click or two tabs.
        builder.HasIndex(w => new { w.UserId, w.ProductId }, "IX_WishlistItems_UserId_ProductId").IsUnique();

        // The list page reads a user's rows newest first.
        builder.HasIndex(w => new { w.UserId, w.CreatedAt }, "IX_WishlistItems_UserId_CreatedAt");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product index for the cascade delete.
        builder.HasIndex(w => w.ProductId, "IX_WishlistItems_ProductId");
    }
}
