using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook.Specifications;

/// <summary>
/// The owner's address book for display: default first, then the most recently edited. No-tracking,
/// because the read repository is scoped to the whole Blazor circuit and a tracked row would go stale.
/// Ownership is part of the WHERE (R-16 D-14).
/// </summary>
public sealed class AddressesByUserSpec : Specification<CustomerAddress>
{
    public AddressesByUserSpec(string userId)
    {
        Query.AsNoTracking()
             .Where(a => a.UserId == userId)
             .OrderByDescending(a => a.IsDefault)
             .ThenByDescending(a => a.UpdatedAt)
             .ThenByDescending(a => a.Id);
    }
}
