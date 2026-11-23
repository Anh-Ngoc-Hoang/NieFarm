using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NieFarm.Domain.Entities;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Infrastructure.Data.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");
        builder.HasKey(a => a.Id);

        // Match AspNetUsers.Id (nvarchar(450)), as CartConfiguration does.
        builder.Property(a => a.UserId).IsRequired().HasMaxLength(CustomerAddress.UserIdMaxLength);
        builder.Property(a => a.RecipientName).IsRequired().HasMaxLength(CustomerAddress.RecipientNameMaxLength);
        builder.Property(a => a.Phone).IsRequired().HasMaxLength(CustomerAddress.PhoneMaxLength);
        builder.Property(a => a.Street).IsRequired().HasMaxLength(CustomerAddress.StreetMaxLength);
        builder.Property(a => a.ProvinceName).IsRequired().HasMaxLength(CustomerAddress.ProvinceNameMaxLength);
        builder.Property(a => a.WardName).IsRequired().HasMaxLength(CustomerAddress.WardNameMaxLength);

        // Both indexes are named: EF folds two unnamed-vs-named HasIndex calls on the same property into one
        // index, which would silently drop the plain lookup index (the filtered one cannot serve non-default rows).
        builder.HasIndex(a => a.UserId, "IX_CustomerAddresses_UserId");

        // The single-default invariant's database backstop (D-12): at most one row per user can have
        // IsDefault = 1. The save handler clears the old default in its own SaveChanges before
        // marking the new one, so this index is never asked to hold two 1s mid-batch.
        builder.HasIndex(a => a.UserId, "IX_CustomerAddresses_UserId_Default")
            .IsUnique()
            .HasFilter("[IsDefault] = 1");

        // An address book is the customer's own data: deleting the account deletes it.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
