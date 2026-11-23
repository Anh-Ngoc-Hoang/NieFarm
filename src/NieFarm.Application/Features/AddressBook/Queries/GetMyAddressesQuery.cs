using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.AddressBook.Dtos;

namespace NieFarm.Application.Features.AddressBook.Queries;

/// <summary>The signed-in customer's own address book (user id from ICurrentUser, never the caller).</summary>
public record GetMyAddressesQuery : IRequest<Result<IReadOnlyList<CustomerAddressDto>>>;
