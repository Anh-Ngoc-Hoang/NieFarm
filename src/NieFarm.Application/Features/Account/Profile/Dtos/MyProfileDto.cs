namespace NieFarm.Application.Features.Account.Profile.Dtos;

/// <summary>
/// The signed-in customer's own profile. Carries no user id, roles or security data. Email is
/// read-only (R-16 D-07).
/// </summary>
public sealed record MyProfileDto(string FullName, string Email, string? PhoneNumber);
