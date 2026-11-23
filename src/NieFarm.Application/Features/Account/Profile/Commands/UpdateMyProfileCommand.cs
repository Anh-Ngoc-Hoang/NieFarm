using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Account.Profile.Commands;

/// <summary>
/// Edits the signed-in customer's name and phone. Email is not editable (R-16 D-07) and the
/// security stamp is deliberately untouched (D-06), so the session stays valid.
/// </summary>
public record UpdateMyProfileCommand(string FullName, string? PhoneNumber) : IRequest<Result>;
