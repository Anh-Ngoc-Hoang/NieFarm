
namespace NieFarm.Web.Content;

/// <summary>
/// Placeholder copy and imagery lifted from the Stitch design
/// (project "NieFarm", id 3649571720089026821) so the public pages render as designed
/// before the catalog exists.
///
/// This is presentation fixture data, not business logic: every list here is replaced
/// by a MediatR query returning DTOs once the corresponding feature lands. Nothing in
/// this file should grow rules, pricing, or persistence.
/// </summary>
public static partial class SiteContent
{
    /// <summary>Formats an amount as Vietnamese đồng, e.g. <c>185.000 ₫</c>.</summary>
    public static string Vnd(decimal amount) => Money.Vnd(amount);

    // -- Products ---------------------------------------------------------

    public sealed record ProductCard(
        string Slug,
        string Name,
        string Category,
        decimal Price,
        string ImageUrl,
        string ImageAlt,
        double Rating = 5,
        int ReviewCount = 0,
        string? Badge = null,
        bool BadgeIsAccent = false,
        string? Summary = null);

    /// <summary>The four cards in the "Sản phẩm nổi bật" band on the home page.</summary>
    public static readonly ProductCard[] FeaturedProducts =
    [
        new("robusta-honey-dac-biet", "Robusta Honey Đặc Biệt", "Robusta - Medium Roast", 185_000,
            "/images/home-3.jpg", "Túi cà phê Robusta Honey Đặc Biệt của Nie Farm"),
        new("arabica-cau-dat", "Arabica Cầu Đất", "Arabica - Light Roast", 250_000,
            "/images/home-4.jpg", "Túi cà phê Arabica Cầu Đất của Nie Farm"),
        new("signature-espresso-blend", "Signature Espresso Blend", "Blend - Dark Roast", 210_000,
            "/images/home-5.jpg", "Túi cà phê Signature Espresso Blend của Nie Farm"),
        new("ca-phe-drip-bag", "Cà phê Drip Bag (Hộp 10 gói)", "Tiện lợi", 150_000,
            "/images/home-6.jpg", "Hộp cà phê Drip Bag 10 gói của Nie Farm")
    ];

    // -- Home page --------------------------------------------------------

    public static readonly string[] MissionPoints =
    [
        "Cải thiện năng suất và chất lượng bền vững.",
        "Áp dụng công nghệ sơ chế hiện đại.",
        "Kết nối thị trường cao cấp, nâng cao giá trị."
    ];

    public sealed record Testimonial(string Quote, string Name, string Role);

    public static readonly Testimonial[] Testimonials =
    [
        new("Cà phê của Nie Farm thực sự khác biệt, hương vị đậm đà và quy trình minh bạch khiến tôi rất " +
            "yên tâm khi sử dụng cho quán của mình.", "Nguyễn Văn A", "Chủ chuỗi Cafe"),
        new("Tôi đánh giá cao sự chuyên nghiệp trong cách Nie Farm hỗ trợ kỹ thuật sơ chế. Chất lượng hạt " +
            "nhân xanh cải thiện rõ rệt qua từng mùa vụ.", "Trần Thị B", "Chủ trang trại cà phê"),
        new("Dịch vụ tận tâm và sản phẩm chất lượng. Nie Farm không chỉ bán cà phê, họ bán cả niềm tin và " +
            "sự tử tế trong từng hạt đậu.", "Lê Văn C", "Khách hàng cá nhân")
    ];

    public static readonly string[] Partners =
    [
        "Logo Partner 1", "Logo Partner 2", "Logo Partner 3", "Logo Partner 4", "Logo Partner 5"
    ];

    // -- Process ----------------------------------------------------------

    public sealed record ProcessStep(int Number, string Title, string Body, string ImageUrl, string ImageAlt);

    public static readonly ProcessStep[] ProcessSteps =
    [
        new(1, "Thu Hoạch Chín 100%",
            "Chúng tôi chỉ thu hái thủ công những quả cà phê chín mọng. Tỷ lệ chín đạt 100% đảm bảo hàm " +
            "lượng đường cao nhất, tạo tiền đề cho hương vị phong phú và độ ngọt tự nhiên trong quá trình " +
            "sơ chế.",
            "/images/process-2.jpg",
            "Đôi tay đeo găng chọn hái những quả cà phê chín đỏ trên cây trong vườn hữu cơ"),
        new(2, "Sơ Chế Chuyên Sâu",
            "Áp dụng đa dạng phương pháp sơ chế (Washed, Honey, Natural) tùy thuộc vào đặc tính mùa vụ. " +
            "Nguồn nước sử dụng được lọc sạch, và quá trình lên men được kiểm soát pH và nhiệt độ chặt chẽ.",
            "/images/process-3.jpg",
            "Hạt cà phê được rửa trong bồn thép không gỉ hiện đại với dòng nước sạch"),
        new(3, "Phơi Giàn Trong Nhà Màng",
            "Hạt cà phê được phơi trên giàn phơi cách đất trong hệ thống nhà màng khép kín. Điều này giúp " +
            "tránh hoàn toàn côn trùng, bụi bẩn, và kiểm soát tốc độ bốc hơi nước để hạt giữ được cấu trúc " +
            "hương vị nguyên bản.",
            "/images/process-4.jpg",
            "Những giàn phơi cà phê thóc vàng óng bên trong nhà màng sáng sủa"),
        new(4, "Rang Xay Kiểm Soát Nhiệt",
            "Sử dụng công nghệ rang hồi khí tiên tiến, thợ rang ghi nhận và theo dõi hồ sơ nhiệt (Roast " +
            "Profile) bằng phần mềm. Đảm bảo mỗi mẻ rang đều phát triển tối đa hương vị đặc trưng mà không " +
            "bị cháy khét hay ám khói.",
            "/images/process-5.jpg",
            "Máy rang cà phê công nghiệp hiện đại đang vận hành cùng màn hình theo dõi đường cong nhiệt")
    ];

    // -- Knowledge / blog --------------------------------------------------
    //    Article types and the article set live in SiteContent.Articles.cs.

    // -- Activities are now a persisted Activity aggregate; see
    //    Application/Features/Activities and /hoat-dong. --------------------

    // -- Order lookup -----------------------------------------------------

    public sealed record TrackedOrderLine(
        string Name,
        string Variant,
        int Quantity,
        decimal UnitPrice,
        string ImageUrl,
        string ImageAlt);

    public sealed record TrackingStep(string Label, string Timestamp, string Icon, bool Reached);

    public sealed record TrackedOrder(
        string Code,
        string PlacedOn,
        string Status,
        string RecipientName,
        string RecipientPhone,
        string ShippingAddress,
        TrackingStep[] Steps,
        TrackedOrderLine[] Lines,
        decimal ShippingFee,
        decimal Discount)
    {
        public decimal Subtotal => Lines.Sum(l => l.UnitPrice * l.Quantity);

        public decimal Total => Subtotal + ShippingFee - Discount;
    }

    public const string SampleOrderCode = "NF12345";

    public static readonly TrackedOrder SampleOrder = new(
        Code: SampleOrderCode,
        PlacedOn: "24/10/2024",
        Status: "Đang giao hàng",
        RecipientName: "Nguyễn Văn A",
        RecipientPhone: "0912 345 678",
        ShippingAddress: "Tòa nhà Bitexco, Số 2 Hải Triều, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
        Steps:
        [
            new("Đang xử lý", "24/10, 09:00", "inventory_2", true),
            new("Đang giao", "25/10, 14:30", "local_shipping", true),
            new("Hoàn thành", "Dự kiến 27/10", "task_alt", false)
        ],
        Lines:
        [
            new("Cà phê Rang Mộc Arabica Cầu Đất", "Gói 500g, Rang Vừa", 2, 250_000,
                "/images/order-lookup-1.jpg", "Túi cà phê rang mộc trên bàn gỗ dưới nắng sớm"),
            new("Bộ Dụng Cụ Pha V60 Cao Cấp", "Màu sắc: Đen nhám", 1, 850_000,
                "/images/order-lookup-2.jpg", "Bộ phễu lọc V60 và bình thủy tinh trên mặt bàn sáng")
        ],
        ShippingFee: 35_000,
        Discount: 35_000);
}
