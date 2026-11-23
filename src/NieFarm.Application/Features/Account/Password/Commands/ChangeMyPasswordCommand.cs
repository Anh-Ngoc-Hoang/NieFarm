using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Account.Password.Commands;

/// <summary>
/// Changes the signed-in customer's password (R-16 D-05, D-08). It runs from the
/// ManageAccount Razor Page, not a circuit, because it changes the security stamp and the auth
/// cookie must be refreshed on a real HTTP response. Results: Unauthorized (not signed in),
/// Forbidden (locked out), Invalid (a ValidationError whose Identifier is Current, New,
/// NewSameAsCurrent or Confirm), Success.
/// </summary>
public record ChangeMyPasswordCommand(string Current, string New, string Confirm) : IRequest<Result>
{
    // Never let the secrets reach a log line or exception message through the record's ToString.
    public override string ToString() => nameof(ChangeMyPasswordCommand);
}
