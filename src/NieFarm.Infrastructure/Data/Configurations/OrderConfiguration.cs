using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Code).IsRequired().HasMaxLength(30);
        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(150);
        builder.Property(o => o.Phone).IsRequired().HasMaxLength(30);
        builder.Property(o => o.Email).HasMaxLength(256);
        builder.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
        builder.Property(o => o.Note).HasMaxLength(1000);

        // Identity's AspNetUsers.Id is nvarchar(450); match it so the FK can be created.
        builder.Property(o => o.CustomerId).HasMaxLength(450);

        builder.Property(o => o.PaymentMethod).HasConversion<int>();

        // Guest orders have no account. Deleting a customer must never delete the order record,
        // so the link is severed instead — the order keeps its own name/phone/address snapshot.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.ShippingFee).HasPrecision(18, 2);
        builder.Property(o => o.Discount).HasPrecision(18, 2);

        // Subtotal and Total are computed from the lines, never stored.
        builder.Ignore(o => o.Subtotal);
        builder.Ignore(o => o.Total);

        builder.Property(o => o.Status).HasConversion<int>();

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // The aggregate exposes its lines read-only, so EF reads and writes the backing field.
        builder.Metadata
            .FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.Code).IsUnique();
        builder.HasIndex(o => o.CreatedAt);
    }
}
