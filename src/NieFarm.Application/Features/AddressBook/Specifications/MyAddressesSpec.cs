using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook.Specifications;

/// <summary>
/// Every address of one user, tracked, for the write handlers: the default logic needs the whole
/// book (at most <see cref="CustomerAddress.MaxPerUser"/> rows) and the target is picked from it by
/// id, so a foreign id is simply absent from the list and answers NotFound (R-16 D-14).
/// </summary>
public sealed class MyAddressesSpec : Specification<CustomerAddress>
{
    public MyAddressesSpec(string userId)
    {
        Query.Where(a => a.UserId == userId)
             .OrderByDescending(a => a.UpdatedAt)
             .ThenByDescending(a => a.Id);
    }
}
