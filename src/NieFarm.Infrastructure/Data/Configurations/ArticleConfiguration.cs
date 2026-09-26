using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(250);
        builder.Property(a => a.Slug).IsRequired().HasMaxLength(270);
        builder.Property(a => a.Author).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Summary).IsRequired().HasMaxLength(600);
        builder.Property(a => a.ImageUrl).HasMaxLength(500);
        builder.Property(a => a.ImageAlt).HasMaxLength(300);
        builder.Property(a => a.HeroImageUrl).HasMaxLength(500);
        builder.Property(a => a.HeroImageAlt).HasMaxLength(300);

        builder.HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.Slug).IsUnique();
        builder.HasIndex(a => a.PublishedOn);
    }
}
