using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Seed;

/// <summary>
/// The single strategic partner transcribed from the deleted strategic-partner fixture that used to
/// live in <c>NieFarm.Web/Content/</c> (Infrastructure cannot reference Web). Used once by <see cref="Seeder"/> to populate an
/// empty Partners table. The website was confirmed by the user on 2026-09-29. No logo — the name
/// renders as a text wordmark; any future seeded logo must live at the top level of
/// <c>wwwroot/images/</c> (never under <c>images/partners/</c>) so an admin action can never delete it.
/// </summary>
public static class PartnerSeedData
{
    public static IReadOnlyList<Partner> Build() =>
    [
        Partner.Create(
            name: "Cà Phê Văn Phòng",
            logoUrl: string.Empty,
            websiteUrl: "https://caphevanphong.vn",
            sortOrder: 0,
            isVisible: true),
    ];
}
