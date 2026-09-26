using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Accounts.Commands;

public record DeleteAccountCommand(string Id) : IRequest<Result>;
