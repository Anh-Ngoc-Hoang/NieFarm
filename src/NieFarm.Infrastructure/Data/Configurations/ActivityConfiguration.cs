using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Slug).IsRequired().HasMaxLength(220);
        builder.Property(a => a.ModalTitle).HasMaxLength(250);
        builder.Property(a => a.ImageUrl).HasMaxLength(500);
        builder.Property(a => a.ImageAlt).HasMaxLength(300);
        builder.Property(a => a.CardSize).HasConversion<int>();

        builder.HasIndex(a => a.Slug).IsUnique();
        builder.HasIndex(a => a.EventDate);
    }
}
