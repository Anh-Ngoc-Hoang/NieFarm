using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Account.Commands;

public record RegisterCustomerCommand(
    string Email, string FullName, string Password, string ConfirmPassword, string? PhoneNumber)
    : IRequest<Result<string>>;
