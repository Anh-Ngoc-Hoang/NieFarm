using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Accounts.Commands;

/// <summary>A blank <paramref name="Password"/> leaves the existing password untouched.</summary>
public record UpdateAccountCommand(
    string Id,
    string FullName,
    string? Password,
    string Role,
    bool IsActive,
    string? PhoneNumber) : IRequest<Result>;
