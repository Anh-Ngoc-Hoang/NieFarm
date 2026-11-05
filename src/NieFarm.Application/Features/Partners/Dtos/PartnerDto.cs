namespace NieFarm.Application.Features.Partners.Dtos;

/// <summary>
/// A public-facing partner. <see cref="LogoUrl"/> is null when the row has no logo (the name is
/// then shown as a text wordmark) and <see cref="WebsiteUrl"/> is null when it has no link.
/// </summary>
public record PartnerDto(int Id, string Name, string? LogoUrl, string? WebsiteUrl);
