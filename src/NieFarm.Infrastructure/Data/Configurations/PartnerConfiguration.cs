using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(Partner.NameMaxLength);
        builder.Property(p => p.LogoUrl).HasMaxLength(Partner.LogoUrlMaxLength);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(Partner.WebsiteUrlMaxLength);

        builder.HasIndex(p => new { p.IsVisible, p.SortOrder });
    }
}
