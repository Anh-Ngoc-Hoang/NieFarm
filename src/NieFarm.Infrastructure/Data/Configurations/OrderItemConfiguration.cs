using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Variant).HasMaxLength(150);
        builder.Property(i => i.ImageUrl).HasMaxLength(500);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);

        builder.Ignore(i => i.LineTotal);

        builder.HasIndex(i => i.OrderId);
    }
}
