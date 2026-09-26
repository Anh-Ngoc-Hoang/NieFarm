using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        // Identity key width (AspNetUsers.Id is nvarchar(450)).
        builder.Property(r => r.AuthorUserId).IsRequired().HasMaxLength(450);
        builder.Property(r => r.AuthorName).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Comment).IsRequired().HasMaxLength(Review.CommentMaxLength);
        builder.Property(r => r.ModeratedByUserId).HasMaxLength(450);
        builder.Property(r => r.CommentEditedByUserId).HasMaxLength(450);

        builder.Property(r => r.Status).HasConversion<int>();

        // Cascade is required, not optional: DeleteProductCommandHandler deletes products with
        // no FK guard, so Restrict would turn an existing admin action into a runtime failure.
        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Public list + average.
        builder.HasIndex(r => new { r.ProductId, r.Status });
        // Moderation queue + pending count.
        builder.HasIndex(r => new { r.Status, r.CreatedAt });
        // Own-pending lookup (D5 guard).
        builder.HasIndex(r => new { r.AuthorUserId, r.ProductId });
    }
}
