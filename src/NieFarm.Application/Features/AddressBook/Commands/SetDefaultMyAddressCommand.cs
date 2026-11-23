using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.AddressBook.Commands;

/// <summary>Makes one of the signed-in customer's addresses the default. Idempotent when it already is.</summary>
public record SetDefaultMyAddressCommand(int Id) : IRequest<Result>;
