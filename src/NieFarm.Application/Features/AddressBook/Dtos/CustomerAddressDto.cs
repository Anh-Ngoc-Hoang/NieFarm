namespace NieFarm.Application.Features.AddressBook.Dtos;

/// <summary>
/// One saved address as the owner sees it. Deliberately carries no user id (R-16 D-14): the
/// caller already is the owner, and an id that is never exposed can never be echoed back as input.
/// </summary>
public record CustomerAddressDto(
    int Id,
    string RecipientName,
    string Phone,
    string Street,
    int? ProvinceCode,
    string ProvinceName,
    int? WardCode,
    string WardName,
    bool IsDefault);
