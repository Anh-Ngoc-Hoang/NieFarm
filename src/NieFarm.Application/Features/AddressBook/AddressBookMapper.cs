using NieFarm.Application.Features.AddressBook.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.AddressBook;

internal static class AddressBookMapper
{
    public static CustomerAddressDto ToDto(CustomerAddress a) => new(
        a.Id, a.RecipientName, a.Phone, a.Street, a.ProvinceCode, a.ProvinceName, a.WardCode, a.WardName, a.IsDefault);
}
