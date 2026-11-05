using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Infrastructure.Data.Seed;

/// <summary>
/// The three "Vietnam Coffee Stories" regions seeded so the home page's map is never blank. One
/// row per <see cref="CoffeeRegionKey"/>; Seeder inserts only the keys that are missing, so admin
/// edits are never overwritten.
///
/// The copy is real Vietnamese coffee content with rounded, hedged figures, but areas, altitudes
/// and history still need farm/editor review (docs/plans/home-vietnam-coffee-stories.md §6). The
/// photos are AI stand-ins from /quy-trinh, kept at the top-level /images/ path (not under the
/// "coffee-regions" upload subfolder) so they are never admin-deletable; their alt texts are
/// copied from the matching SiteContent.ProcessSteps entries because those describe what is
/// actually in the picture.
/// </summary>
public static class CoffeeRegionSeedData
{
    public static IReadOnlyList<CoffeeRegion> Build()
    {
        var north = CoffeeRegion.Create(
            CoffeeRegionKey.North,
            title: "Arabica trên triền núi Tây Bắc",
            tagline: "Sơn La, Điện Biên — mùa đông lạnh, sương muối và những vườn Catimor trên cao.",
            description:
                "Cà phê theo chân người Pháp đến Việt Nam từ thế kỷ XIX; ở miền Bắc ngày nay, các cao " +
                "nguyên Sơn La và Điện Biên là vùng Arabica lớn nhất.\n\n" +
                "Vườn nằm trên những triền đồi dốc, mùa đông lạnh và khô, đôi khi có sương muối. Quả chín " +
                "chậm trong tiết trời mát nên tích luỹ nhiều đường, cho vị chua thanh và hậu ngọt dịu.",
            altitude: "Khoảng 600–1.500 m",
            varieties: "Arabica (chủ yếu Catimor)",
            climate: "Cận nhiệt đới gió mùa, đông lạnh và khô, có sương muối",
            flavorProfile: "Chua thanh, thân nhẹ, hậu ngọt caramel",
            imageUrl: "/images/quy-trinh-6.jpg",
            imageAlt: "Những giàn phơi cà phê thóc vàng óng bên trong nhà màng sáng sủa");
        north.SetAreas(
        [
            (null, "Sơn La (Thuận Châu, Mai Sơn)", "Vùng Arabica lớn nhất miền Bắc, vườn đồi dốc."),
            (null, "Mường Ảng, Điện Biên", "Arabica Catimor trên triền đồi, thu hoạch từ cuối năm."),
        ]);

        var central = CoffeeRegion.Create(
            CoffeeRegionKey.Central,
            title: "Khe Sanh — nơi Arabica gặp Robusta",
            tagline: "Dọc sườn Trường Sơn, cà phê gắn với những đồn điền từ thời Pháp thuộc.",
            description:
                "Dải đất miền Trung hẹp và nhiều nắng gió, nhưng ở phía tây, dọc dãy Trường Sơn, vùng " +
                "Khe Sanh – Hướng Hóa (Quảng Trị) nổi tiếng với cà phê từ thời Pháp thuộc.\n\n" +
                "Khí hậu mang dấu ấn của cả hai sườn Trường Sơn: mưa dồn về cuối năm, mùa hè có gió Lào " +
                "khô nóng. Arabica và Robusta được trồng trên đất đỏ bazan, cho tách cà phê đậm vừa, hậu " +
                "vị sô-cô-la.",
            altitude: "Khoảng 400–700 m",
            varieties: "Arabica Catimor, Robusta",
            climate: "Nhiệt đới gió mùa, giao thoa hai sườn Trường Sơn",
            flavorProfile: "Đậm vừa, hậu sô-cô-la, chua nhẹ",
            imageUrl: "/images/quy-trinh-2.jpg",
            imageAlt: "Những hàng cây cà phê con dưới lưới che nắng với đường ống tưới nhỏ giọt dọc luống");
        central.SetAreas(
        [
            (null, "Khe Sanh – Hướng Hóa, Quảng Trị", "Arabica và Robusta trên đất bazan, vùng cà phê nổi tiếng nhất miền Trung."),
            (null, "Phủ Quỳ, Nghệ An", "Vùng đồi đất đỏ, nơi cà phê được trồng từ đầu thế kỷ XX."),
        ]);

        var south = CoffeeRegion.Create(
            CoffeeRegionKey.South,
            title: "Tây Nguyên — thủ phủ Robusta",
            tagline: "Cao nguyên đất đỏ bazan làm nên phần lớn sản lượng cà phê Việt Nam.",
            description:
                "Tây Nguyên và Đông Nam Bộ là trái tim của ngành cà phê Việt Nam — quốc gia sản xuất cà " +
                "phê lớn thứ hai thế giới và dẫn đầu về Robusta. Phần lớn diện tích cà phê cả nước nằm " +
                "trên các cao nguyên đất đỏ bazan quanh Buôn Ma Thuột, Pleiku và Di Linh.\n\n" +
                "Ở độ cao 400–800 m, Robusta cho vị đậm, đắng sô-cô-la. Lên tới trên 1.500 m quanh Cầu " +
                "Đất và Đà Lạt (Lâm Đồng), khí hậu mát quanh năm là nơi của những lô Arabica chua thanh, " +
                "hương hoa quả.",
            altitude: "Khoảng 400–1.600 m",
            varieties: "Robusta (chủ đạo), Arabica tại Lâm Đồng",
            climate: "Cao nguyên nhiệt đới, mùa mưa – mùa khô rõ rệt",
            flavorProfile: "Robusta đậm, đắng sô-cô-la; Arabica chua thanh, hương hoa quả",
            imageUrl: "/images/quy-trinh-1.jpg",
            imageAlt: "Người nông dân đội nón lá tra hạt vào bầu đất trong vườn ươm cà phê dưới lưới che");
        south.SetAreas(
        [
            (null, "Buôn Ma Thuột, Đắk Lắk", "Thủ phủ Robusta, đất đỏ bazan, khoảng 500 m."),
            (null, "Cầu Đất – Đà Lạt, Lâm Đồng", "Arabica trên 1.500 m, mát và nhiều sương quanh năm."),
            (null, "Di Linh – Bảo Lộc, Lâm Đồng", "Robusta trên cao nguyên Di Linh, khoảng 800–1.000 m."),
            (null, "Pleiku, Gia Lai", "Robusta trên cao nguyên đất đỏ bazan."),
            (null, "Đắk Mil, Đắk Nông (nay thuộc Lâm Đồng)", "Robusta cao nguyên; sáp nhập vào tỉnh Lâm Đồng từ 7/2025."),
            (null, "Đắk Hà, Kon Tum (nay thuộc Quảng Ngãi)", "Robusta vùng bắc Tây Nguyên; sáp nhập vào Quảng Ngãi từ 7/2025."),
            (null, "Đồng Nai – Bình Phước", "Robusta vùng Đông Nam Bộ, độ cao thấp; Bình Phước sáp nhập vào Đồng Nai từ 7/2025."),
        ]);

        return [north, central, south];
    }
}
