using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class OrderStatusChangeConfiguration : IEntityTypeConfiguration<OrderStatusChange>
{
    public void Configure(EntityTypeBuilder<OrderStatusChange> builder)
    {
        builder.ToTable("OrderStatusChanges");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FromStatus).HasConversion<int?>();
        builder.Property(h => h.ToStatus).HasConversion<int>();
        builder.Property(h => h.ChangedBy).HasConversion<int>();

        // Plain audit columns: deliberately no FK to AspNetUsers, so deleting an account never
        // blocks or erases the history (same precedent as Review.ModeratedByUserId).
        builder.Property(h => h.ChangedByUserId).HasMaxLength(450);
        builder.Property(h => h.ChangedByName).HasMaxLength(OrderStatusChange.NameMaxLength);
        builder.Property(h => h.Note).HasMaxLength(OrderStatusChange.NoteMaxLength);

        builder.HasIndex(h => new { h.OrderId, h.ChangedAt });
    }
}
