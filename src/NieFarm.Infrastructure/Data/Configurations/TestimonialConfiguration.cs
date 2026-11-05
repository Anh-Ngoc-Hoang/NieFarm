using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Quote).IsRequired().HasMaxLength(Testimonial.QuoteMaxLength);
        builder.Property(t => t.AuthorName).IsRequired().HasMaxLength(Testimonial.AuthorNameMaxLength);
        builder.Property(t => t.AuthorRole).HasMaxLength(Testimonial.AuthorRoleMaxLength);
        builder.Property(t => t.PhotoUrl).HasMaxLength(Testimonial.PhotoUrlMaxLength);

        builder.HasIndex(t => new { t.IsVisible, t.SortOrder });
    }
}
