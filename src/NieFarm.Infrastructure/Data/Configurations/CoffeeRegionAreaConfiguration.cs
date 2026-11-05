using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Configurations;

/// <summary>
/// No unique index on (CoffeeRegionId, Name): CoffeeRegion.SetAreas updates rows in place, so a
/// rename swap (A to B and B to A in one save) would trip a unique index mid-batch — the same
/// reasoning as CropConfiguration. Name uniqueness within a region is enforced by the validator
/// and the domain instead.
/// </summary>
public class CoffeeRegionAreaConfiguration : IEntityTypeConfiguration<CoffeeRegionArea>
{
    public void Configure(EntityTypeBuilder<CoffeeRegionArea> builder)
    {
        builder.ToTable("CoffeeRegionAreas");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(CoffeeRegionArea.NameMaxLength);
        builder.Property(a => a.Note).HasMaxLength(CoffeeRegionArea.NoteMaxLength);

        builder.HasIndex(a => new { a.CoffeeRegionId, a.SortOrder });
    }
}
