using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class GatewayTransactionConfiguration : IEntityTypeConfiguration<GatewayTransaction>
{
    public void Configure(EntityTypeBuilder<GatewayTransaction> builder)
    {
        builder.ToTable("GatewayTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Provider).IsRequired().HasMaxLength(GatewayTransaction.ProviderMaxLength);
        builder.Property(t => t.ExternalId).IsRequired().HasMaxLength(GatewayTransaction.ExternalIdMaxLength);
        builder.Property(t => t.TransferType).IsRequired().HasMaxLength(GatewayTransaction.TransferTypeMaxLength);
        builder.Property(t => t.AccountNumber).IsRequired().HasMaxLength(GatewayTransaction.AccountNumberMaxLength);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.Content).IsRequired().HasMaxLength(GatewayTransaction.ContentMaxLength);
        builder.Property(t => t.Code).HasMaxLength(GatewayTransaction.CodeMaxLength);
        builder.Property(t => t.Reference).HasMaxLength(GatewayTransaction.ReferenceMaxLength);
        builder.Property(t => t.Outcome).HasConversion<int>();

        // OrderId is a plain column with no foreign key: the audit survives order deletion.
        builder.Property(t => t.OrderId);

        // Idempotency backstop: a second delivery of the same transaction fails its save.
        builder.HasIndex(t => new { t.Provider, t.ExternalId }).IsUnique();
    }
}
