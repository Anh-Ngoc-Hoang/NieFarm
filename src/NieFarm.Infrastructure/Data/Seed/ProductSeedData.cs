using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Seed;

/// <summary>
/// The five products referenced across the original Stitch-design fixtures
/// (<c>NieFarm.Web/Content/SiteContent.ShopProducts</c> and <c>RelatedProducts</c>),
/// transcribed here because Infrastructure cannot reference Web. Used by <see cref="Seeder"/>,
/// per-slug idempotent, to populate <c>/cua-hang</c> and the "Sản phẩm liên quan" band on a
/// freshly migrated site. Category ids are resolved by <see cref="Seeder"/> before calling
/// <see cref="Build"/> since they are assigned by the database, not known at compile time.
///
/// <c>robusta-honey-dac-biet</c> additionally carries the full showcase content transcribed
/// from the old <c>SiteContent.Showcase</c> fixture before it was deleted (specs, features,
/// package/grind options and a gallery), so <c>/san-pham/robusta-honey-dac-biet</c> is not blank
/// once that page reads real data. Its <c>oldPrice</c> (210_000) is a display-only "was" price
/// with no promotion semantics — see docs/plans/shop-and-product-detail-real-data.md, D4. Every
/// other seeded product passes <c>oldPrice: null</c>.
/// </summary>
public static class ProductSeedData
{
    public static IReadOnlyList<Product> Build(IReadOnlyDictionary<string, int> categoryIdByName)
    {
        var robusta = Product.Create(
            name: "Robusta Honey Đặc Biệt",
            slug: "robusta-honey-dac-biet",
            categoryId: categoryIdByName["Robusta"],
            price: 185_000,
            oldPrice: 210_000,
            imageUrl: "/images/shop-1.jpg",
            imageAlt: "Túi cà phê Robusta rang mộc đặt trên mặt gỗ sáng màu, ánh sáng tự nhiên ấm áp",
            summary: ToHtml(
                "Dòng sản phẩm cà phê Robusta chế biến phương pháp Honey, mang lại hương vị đậm đà đặc " +
                "trưng của đại ngàn Tây Nguyên xen lẫn hậu vị ngọt ngào của mật ong rừng."),
            description: ToHtml(
                "Robusta Honey là sự kết hợp hoàn hảo giữa kỹ thuật sơ chế mật ong (Honey Process) và " +
                "những hạt cà phê Robusta chất lượng cao nhất từ nông trại Nie Farm tại Đắk Lắk. Phương " +
                "pháp này giữ lại lớp chất nhầy tự nhiên của vỏ quả cà phê trong quá trình phơi khô, giúp " +
                "hạt cà phê hấp thụ vị ngọt tự nhiên, tạo nên một hương vị độc đáo.\n\n" +
                "Trải nghiệm thưởng thức bắt đầu bằng hương thơm nồng nàn, tiếp theo là vị đậm đà mộc mạc " +
                "đặc trưng của Robusta, nhưng lại vô cùng êm ái, ít chát. Điểm nhấn là hậu vị ngọt kéo dài " +
                "nơi cuống họng, thoang thoảng hương chocolate đen và caramel."),
            badge: "Mới",
            isFeatured: true,
            isVisible: true,
            sortOrder: 0);

        robusta.SetSpecs(
        [
            ("Giống cà phê", "Robusta Đắk Lắk"),
            ("Phương pháp sơ chế", "Honey Process"),
            ("Mức độ rang", "Medium Roast"),
            ("Vùng trồng", "Ea Kao, Buôn Ma Thuột, Đắk Lắk"),
            ("Độ cao", "500 - 700m"),
            ("Hương vị", "Chocolate đen, caramel, hậu vị ngọt"),
            ("Hạn sử dụng", "12 tháng kể từ ngày rang")
        ]);

        robusta.SetFeatures(
        [
            "Rang mộc thủ công theo mẻ nhỏ, không pha trộn",
            "Sơ chế Honey Process giữ trọn vị ngọt tự nhiên",
            "Đóng gói van một chiều, giữ hương thơm lâu dài"
        ]);

        // 3 package sizes × 4 grind levels; price varies only by package size.
        string[] packages = ["250g", "500g", "1kg"];
        string[] grinds =
        [
            "Nguyên hạt (Whole Bean)",
            "Xay pha phin",
            "Xay pha máy (Espresso)",
            "Xay pha Pour Over"
        ];
        decimal[] packagePrices = [185_000, 340_000, 620_000];

        var variants = new List<(IReadOnlyList<int> ValueIndexes, decimal Price, string? ImageUrl, bool IsAvailable)>();
        for (var packageIndex = 0; packageIndex < packages.Length; packageIndex++)
        {
            for (var grindIndex = 0; grindIndex < grinds.Length; grindIndex++)
            {
                variants.Add(([packageIndex, grindIndex], packagePrices[packageIndex], null, true));
            }
        }

        robusta.SetOptionsAndVariants(
            options:
            [
                ("Đơn vị", packages),
                ("Mức độ xay", grinds)
            ],
            variants: variants);

        // Three existing committed design assets that read naturally as extra angles; the
        // primary image stays /images/shop-1.jpg. Unmanaged (not under images/products/), so
        // ManagedImages.IsManagedImage never lets an admin action delete them.
        robusta.SetImages(
        [
            "/images/product-detail-2.jpg",
            "/images/product-detail-3.jpg",
            "/images/product-detail-4.jpg"
        ]);

        var arabica = Product.Create(
            name: "Arabica Cầu Đất",
            slug: "arabica-cau-dat",
            categoryId: categoryIdByName["Arabica"],
            price: 250_000,
            oldPrice: null,
            imageUrl: "/images/shop-2.jpg",
            imageAlt: "Túi cà phê Arabica cạnh phễu lọc gốm trên mặt đá cẩm thạch trắng",
            summary: ToHtml("Hương thơm nồng nàn, vị chua thanh nhẹ nhàng, hậu vị ngọt."),
            description: ToHtml("Hương thơm nồng nàn, vị chua thanh nhẹ nhàng, hậu vị ngọt."),
            badge: "",
            isFeatured: true,
            isVisible: true,
            sortOrder: 1);

        var blend = Product.Create(
            name: "Signature Espresso Blend",
            slug: "signature-espresso-blend",
            categoryId: categoryIdByName["Blend"],
            price: 210_000,
            oldPrice: null,
            imageUrl: "/images/shop-3.jpg",
            imageAlt: "Gói cà phê Espresso Blend trên nền đá xám cùng thìa espresso bằng đồng",
            summary: ToHtml("Phối trộn hoàn hảo 70% Robusta và 30% Arabica, crema dày, đậm đà."),
            description: ToHtml("Phối trộn hoàn hảo 70% Robusta và 30% Arabica, crema dày, đậm đà."),
            badge: "Bán chạy",
            isFeatured: true,
            isVisible: true,
            sortOrder: 2);

        var dripBag = Product.Create(
            name: "Cà phê Drip Bag (Hộp 10 gói)",
            slug: "ca-phe-drip-bag",
            categoryId: categoryIdByName["Tiện lợi"],
            price: 150_000,
            oldPrice: null,
            imageUrl: "/images/product-detail-7.jpg",
            imageAlt: "Bộ cà phê phin giấy đóng gói tối giản đặt cạnh laptop trên bàn gỗ sáng",
            summary: ToHtml("Cà phê phin giấy tiện lợi cho dân văn phòng, hương vị chuẩn quán."),
            description: ToHtml("Cà phê phin giấy tiện lợi cho dân văn phòng, hương vị chuẩn quán."),
            badge: "",
            isFeatured: false,
            isVisible: true,
            sortOrder: 3);

        var coldBrew = Product.Create(
            name: "Cold Brew Concentrate",
            slug: "cold-brew-concentrate",
            categoryId: categoryIdByName["Cold Brew"],
            price: 180_000,
            oldPrice: null,
            imageUrl: "/images/product-detail-8.jpg",
            imageAlt: "Hũ thủy tinh đựng cốt cà phê Cold Brew màu hổ phách trên nền trắng",
            summary: ToHtml("Cốt ủ lạnh 24h, mượt mà, ít axit, sẵn sàng thưởng thức."),
            description: ToHtml("Cốt ủ lạnh 24h, mượt mà, ít axit, sẵn sàng thưởng thức."),
            badge: "",
            isFeatured: false,
            isVisible: true,
            sortOrder: 4);

        return [robusta, arabica, blend, dripBag, coldBrew];
    }

    /// <summary>
    /// Wraps plain fixture text in paragraph markup, since Summary/Description are rendered as
    /// MarkupString on the public site. Splitting on a blank line preserves the original
    /// two-paragraph fixture copy instead of collapsing it into one run-on paragraph.
    /// </summary>
    private static string ToHtml(string plainText) =>
        string.Join(string.Empty, plainText
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(paragraph => $"<p>{paragraph.Trim()}</p>"));
}
