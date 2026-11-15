namespace NieFarm.Application.Features.Shipping;

public record ProvinceEntry(int Code, string Name, IReadOnlyList<string> Aliases);

/// <summary>
/// The 34 provincial-level units after the 2025 reorganisation. Codes and names were copied from
/// https://provinces.open-api.vn/api/v2/p/ (the same service the checkout address picker uses);
/// never guess them. Zones store the Code, never a name, and display names always come from here.
/// Aliases cover the manual-entry fallback: pre-2025 names map to the unit they were merged into
/// (Resolution 202/2025/QH15) plus a few short forms. When the administrative map changes again,
/// update this list (the admin Shipping page warns when the live API stops matching it).
/// </summary>
public static class VietnamProvinceCatalog
{
    public static IReadOnlyList<ProvinceEntry> All { get; } =
    [
        new(1, "Hà Nội", ["Hanoi"]),
        new(4, "Cao Bằng", []),
        new(8, "Tuyên Quang", ["Hà Giang"]),
        new(11, "Điện Biên", []),
        new(12, "Lai Châu", []),
        new(14, "Sơn La", []),
        new(15, "Lào Cai", ["Yên Bái"]),
        new(19, "Thái Nguyên", ["Bắc Kạn"]),
        new(20, "Lạng Sơn", []),
        new(22, "Quảng Ninh", []),
        new(24, "Bắc Ninh", ["Bắc Giang"]),
        new(25, "Phú Thọ", ["Vĩnh Phúc", "Hòa Bình"]),
        new(31, "Hải Phòng", ["Hải Dương"]),
        new(33, "Hưng Yên", ["Thái Bình"]),
        new(37, "Ninh Bình", ["Hà Nam", "Nam Định"]),
        new(38, "Thanh Hóa", []),
        new(40, "Nghệ An", []),
        new(42, "Hà Tĩnh", []),
        new(44, "Quảng Trị", ["Quảng Bình"]),
        new(46, "Huế", ["Thừa Thiên Huế"]),
        new(48, "Đà Nẵng", ["Quảng Nam"]),
        new(51, "Quảng Ngãi", ["Kon Tum"]),
        new(52, "Gia Lai", ["Bình Định"]),
        new(56, "Khánh Hòa", ["Ninh Thuận"]),
        new(66, "Đắk Lắk", ["Phú Yên", "Đắc Lắc", "Daklak"]),
        new(68, "Lâm Đồng", ["Đắk Nông", "Bình Thuận"]),
        new(75, "Đồng Nai", ["Bình Phước"]),
        new(79, "Hồ Chí Minh", ["Bình Dương", "Bà Rịa - Vũng Tàu", "HCM", "TPHCM", "Sài Gòn", "Saigon"]),
        new(80, "Tây Ninh", ["Long An"]),
        new(82, "Đồng Tháp", ["Tiền Giang"]),
        new(86, "Vĩnh Long", ["Bến Tre", "Trà Vinh"]),
        new(91, "An Giang", ["Kiên Giang"]),
        new(92, "Cần Thơ", ["Sóc Trăng", "Hậu Giang"]),
        new(96, "Cà Mau", ["Bạc Liêu"]),
    ];

    private static readonly Dictionary<int, ProvinceEntry> ByCode = All.ToDictionary(p => p.Code);

    private static readonly Dictionary<string, int> CodeByKey = BuildKeys();

    public static ProvinceEntry? Find(int code) => ByCode.GetValueOrDefault(code);

    public static string DisplayName(int code) => Find(code)?.Name ?? $"#{code}";

    /// <summary>Resolves a typed or API-supplied province name to a catalog code, or null.</summary>
    public static int? TryResolve(string? name)
    {
        var key = ProvinceNameNormalizer.Normalize(name);
        return key.Length > 0 && CodeByKey.TryGetValue(key, out var code) ? code : null;
    }

    private static Dictionary<string, int> BuildKeys()
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var entry in All)
        {
            foreach (var name in entry.Aliases.Prepend(entry.Name))
            {
                var key = ProvinceNameNormalizer.Normalize(name);
                if (map.TryGetValue(key, out var existing) && existing != entry.Code)
                    throw new InvalidOperationException($"Province key '{key}' maps to two codes.");
                map[key] = entry.Code;
            }
        }
        return map;
    }
}
