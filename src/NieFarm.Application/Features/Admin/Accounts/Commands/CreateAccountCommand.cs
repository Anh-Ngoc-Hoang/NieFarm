using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Accounts.Commands;

public record CreateAccountCommand(
    string Email,
    string FullName,
    string Password,
    string Role,
    string? PhoneNumber) : IRequest<Result<string>>;
