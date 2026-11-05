
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

    // -- Home page --------------------------------------------------------
    //    Featured products, testimonials and the strategic partner band are no longer fixtures:
    //    Home.razor reads GetHomeFeaturedProductsQuery / GetTestimonialsQuery / GetPartnersQuery
    //    (real, admin-managed data). MissionPoints and ProcessSteps below remain marketing-copy
    //    fixtures — see
    //    docs/plans/home-redesign-crop-testimonials.md "Not yet set up".

    public static readonly string[] MissionPoints =
    [
        "Cải thiện năng suất và chất lượng bền vững.",
        "Áp dụng công nghệ sơ chế hiện đại.",
        "Kết nối thị trường cao cấp, nâng cao giá trị."
    ];

    // -- Process ----------------------------------------------------------
    //    Transcribed from docs/design-preview/quy-trinh.html:124-366 (NieFarm 2.0
    //    redesign, docs/plans/public-site-redesign-nf2.md §7, step 2.4). "Glyph" is an
    //    NfIcon *name*, never SVG markup — the path data stays in NfIcon.razor, the same
    //    reasoning that put ActivityCardSize in the Domain instead of a height-class
    //    string. The quy-trinh-*.jpg images are AI-generated in Stitch (project "NieFarm"),
    //    one per stage, and are stand-ins until real farm photos exist.

    public sealed record ProcessStep(
        int Number,
        string Anchor,
        string RailLabel,
        string Eyebrow,
        string Title,
        string Body,
        string[] Facts,
        string Glyph,
        string ImageUrl,
        string ImageAlt);

    public static readonly ProcessStep[] ProcessSteps =
    [
        new(1, "s1", "Gieo hạt", "Vườn ươm", "Chọn giống & gieo hạt",
            "Hạt giống được lấy từ những cây mẹ khỏe nhất của vụ trước, ngâm ủ rồi gieo vào bầu đất " +
            "trộn phân hữu cơ hoai mục. Mỗi lô giống đều được ghi nhật ký riêng để truy được nguồn gốc " +
            "về sau.",
            [
                "Giống Arabica Typica & Robusta chọn lọc",
                "Bầu đất không dùng phân hóa học"
            ],
            "seedling",
            "/images/quy-trinh-1.jpg",
            "Người nông dân đội nón lá tra hạt vào bầu đất trong vườn ươm cà phê dưới lưới che"),
        new(2, "s2", "Chăm cây con", "6–8 tháng đầu", "Chăm sóc cây con",
            "Cây con lớn lên dưới lưới che 50% nắng, tưới nhỏ giọt theo độ ẩm đất thay vì theo lịch. " +
            "Chỉ những cây đạt chiều cao và số cặp lá tiêu chuẩn mới được đưa ra vườn trồng.",
            [
                "Tưới nhỏ giọt theo cảm biến độ ẩm",
                "Loại bỏ cây yếu trước khi xuống giống"
            ],
            "sprout",
            "/images/quy-trinh-2.jpg",
            "Những hàng cây cà phê con dưới lưới che nắng với đường ống tưới nhỏ giọt dọc luống"),
        new(3, "s3", "Canh tác hữu cơ", "Quanh năm", "Canh tác hữu cơ",
            "Không thuốc diệt cỏ, không phân bón tổng hợp. Cỏ được cắt chứ không diệt để giữ ẩm và " +
            "chống xói mòn; vỏ cà phê vụ trước được ủ vi sinh và trả lại cho đất. Cây che bóng xen canh " +
            "giúp quả chín chậm và đều hơn.",
            [
                "Phân vi sinh ủ từ vỏ cà phê tại chỗ",
                "Trồng xen cây che bóng bản địa",
                "Không thuốc diệt cỏ trong suốt chu kỳ"
            ],
            "shield-leaf",
            "/images/quy-trinh-3.jpg",
            "Nông dân đội nón lá rải phân ủ từ vỏ cà phê giữa những hàng cà phê xen cây che bóng"),
        new(4, "s4", "Thu hái chín", "Mùa vụ", "Thu hái chín 100%",
            "Chúng tôi hái tay, chọn từng quả, quay lại cùng một cây nhiều lượt trong mùa thay vì tuốt " +
            "một lần. Tỷ lệ chín 100% là điều kiện bắt buộc để có hàm lượng đường cao nhất — nền tảng " +
            "của toàn bộ hương vị phía sau.",
            [
                "Hái chọn nhiều đợt trên cùng một cây",
                "Phân loại nổi/chìm ngay trong ngày hái"
            ],
            "cherries",
            "/images/quy-trinh-4.jpg",
            "Đôi tay đeo găng chọn hái những quả cà phê chín đỏ vào gùi tre trong vườn hữu cơ"),
        new(5, "s5", "Sơ chế", "Trong 12 giờ", "Sơ chế chuyên sâu",
            "Washed, Honey hay Natural — phương pháp được chọn theo đặc tính từng lô, từng mùa. " +
            "Nước sử dụng được lọc sạch; quá trình lên men kiểm soát pH và nhiệt độ, dừng đúng điểm " +
            "thay vì để tự nhiên.",
            [
                "Lên men kiểm soát pH & nhiệt độ",
                "Sơ chế trong vòng 12 giờ sau thu hái"
            ],
            "drop",
            "/images/quy-trinh-5.jpg",
            "Kỹ thuật viên đo pH nước rửa hạt cà phê thóc trong bồn thép không gỉ"),
        new(6, "s6", "Phơi giàn", "15–20 ngày", "Phơi giàn trong nhà màng",
            "Hạt nằm trên giàn cách đất trong nhà màng khép kín, được đảo tay nhiều lần mỗi ngày. " +
            "Không côn trùng, không bụi, không mưa bất chợt — và tốc độ bốc hơi được kéo chậm để hạt " +
            "giữ nguyên cấu trúc hương.",
            [
                "Giàn cách đất, trong nhà màng kín",
                "Dừng ở độ ẩm 12% — đo, không ước lượng"
            ],
            "drying-rack",
            "/images/quy-trinh-6.jpg",
            "Những giàn phơi cà phê thóc vàng óng bên trong nhà màng sáng sủa"),
        new(7, "s7", "Rang xay", "11–14 phút", "Rang kiểm soát nhiệt",
            "Công nghệ rang hồi khí, mỗi mẻ có một hồ sơ nhiệt được phần mềm ghi lại và lặp lại được. " +
            "Mục tiêu là phát triển tối đa hương vị đặc trưng của lô — không cháy khét, không ám khói, " +
            "không giấu lỗi bằng độ rang đậm.",
            [
                "Roast profile lưu và tái lập từng mẻ",
                "Cupping kiểm định trước khi đóng gói"
            ],
            "roaster",
            "/images/quy-trinh-7.jpg",
            "Hạt cà phê vừa rang đổ xuống khay làm nguội, bên cạnh máy tính theo dõi đường cong nhiệt"),
        new(8, "s8", "Vào ly", "Đích đến", "Và cuối cùng, vào ly của bạn",
            "Cà phê được đóng gói có van một chiều, in ngày rang thật, và gửi đi trong vòng 48 giờ. " +
            "Trên mỗi túi là mã lô — nhập vào website là bạn xem lại được đúng hành trình vừa đi qua.",
            [
                "In ngày rang, không in \"hạn sử dụng 2 năm\"",
                "Mã lô truy xuất ngược về vườn"
            ],
            "cup",
            "/images/quy-trinh-8.jpg",
            "Ly cà phê gốm pha thủ công đặt trên bàn gỗ cạnh túi cà phê hạt giấy kraft")
    ];

    /// <summary>The three stats in the "/quy-trinh" hero (08 chặng / 100% / 1.450m) — marketing
    /// copy, not derived from any query (docs/plans/public-site-redesign-nf2.md §2.3).</summary>
    public sealed record ProcessStat(string Value, string Label);

    public static readonly ProcessStat[] ProcessStats =
    [
        new("08", "chặng kiểm soát"),
        new("100%", "quả chín khi hái"),
        new("1.450m", "độ cao canh tác")
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
