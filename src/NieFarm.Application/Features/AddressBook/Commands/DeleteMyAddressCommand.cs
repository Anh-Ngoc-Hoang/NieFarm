using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.AddressBook.Commands;

/// <summary>
/// Deletes one of the signed-in customer's addresses. Deleting the default promotes the most
/// recently edited remaining address (R-16 D-12).
/// </summary>
public record DeleteMyAddressCommand(int Id) : IRequest<Result>;
