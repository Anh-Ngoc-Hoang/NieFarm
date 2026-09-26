namespace NieFarm.Application.Features.Admin.Accounts.Dtos;

public record AccountDto(
    string Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive);
