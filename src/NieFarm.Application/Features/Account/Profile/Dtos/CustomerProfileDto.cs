namespace NieFarm.Application.Features.Account.Profile.Dtos;

/// <summary>What <c>IIdentityService</c> hands back for a customer's own profile.</summary>
public sealed record CustomerProfileDto(string FullName, string Email, string? PhoneNumber);
