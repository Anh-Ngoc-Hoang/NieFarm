using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons", t =>
            t.HasCheckConstraint("CK_Coupons_Type", "[Type] IN (1, 2, 3)"));
        builder.HasKey(c => c.Id);

        // Codes are stored upper-case by the domain; the unique index (case-insensitive collation)
        // is the backstop against a duplicate.
        builder.Property(c => c.Code).IsRequired().HasMaxLength(Coupon.CodeMaxLength);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Description).HasMaxLength(Coupon.DescriptionMaxLength);
        builder.Property(c => c.Type).HasConversion<int>();

        builder.Property(c => c.Value).HasPrecision(18, 2);
        builder.Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);
        builder.Property(c => c.MinOrderSubtotal).HasPrecision(18, 2);

        // Shadow concurrency token (the R-07 Orders lesson: a shadow rowversion needs IsRequired).
        // Every redemption UPDATEs this row, so two orders racing for the last use cannot both commit.
        builder.Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
    }
}
