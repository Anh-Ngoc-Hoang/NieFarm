using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.AddressBook.Commands;

/// <summary>
/// Creates (null <paramref name="Id"/>) or edits one of the signed-in customer's addresses. The owner
/// comes from ICurrentUser. <paramref name="ProvinceCode"/> is optional: with it the catalog supplies
/// the name, without it <paramref name="ProvinceName"/> must resolve through the catalog (R-16 D-13).
/// Returns the saved address id.
/// </summary>
public record SaveMyAddressCommand(
    int? Id,
    string RecipientName,
    string Phone,
    string Street,
    int? ProvinceCode,
    string? ProvinceName,
    int? WardCode,
    string WardName,
    bool MakeDefault) : IRequest<Result<int>>;
