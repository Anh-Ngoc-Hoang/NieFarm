using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class OrderPaymentChangeConfiguration : IEntityTypeConfiguration<OrderPaymentChange>
{
    public void Configure(EntityTypeBuilder<OrderPaymentChange> builder)
    {
        builder.ToTable("OrderPaymentChanges");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FromStatus).HasConversion<int?>();
        builder.Property(h => h.ToStatus).HasConversion<int>();
        builder.Property(h => h.ChangedBy).HasConversion<int>();

        // Plain audit columns, no FK to AspNetUsers (same precedent as OrderStatusChange).
        builder.Property(h => h.ChangedByUserId).HasMaxLength(450);
        builder.Property(h => h.ChangedByName).HasMaxLength(OrderPaymentChange.NameMaxLength);
        builder.Property(h => h.Amount).HasPrecision(18, 2);
        builder.Property(h => h.Reference).HasMaxLength(OrderPaymentChange.ReferenceMaxLength);
        builder.Property(h => h.Provider).HasMaxLength(OrderPaymentChange.ProviderMaxLength);
        builder.Property(h => h.ExternalId).HasMaxLength(OrderPaymentChange.ExternalIdMaxLength);
        builder.Property(h => h.Note).HasMaxLength(OrderPaymentChange.NoteMaxLength);

        builder.HasIndex(h => new { h.OrderId, h.ChangedAt });

        // Webhook idempotency backstop. Deliberately no unique index on Reference: one bank
        // transfer can legitimately cover two orders and manual references are free text.
        builder.HasIndex(h => new { h.Provider, h.ExternalId })
            .IsUnique()
            .HasFilter("[ExternalId] IS NOT NULL");
    }
}
