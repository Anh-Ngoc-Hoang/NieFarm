namespace NieFarm.Web.Content;

/// <summary>
/// Knowledge-section fixture data: the article set behind /kien-thuc and /bai-viet/{slug},
/// lifted from the Stitch screens. Replaced by blog queries when that feature lands.
/// </summary>
public static partial class SiteContent
{
    public enum ArticleBlockKind
    {
        Paragraph,
        Heading2,
        Heading3,
        BulletList,
        Image
    }

    /// <summary>
    /// One block of article body copy. <see cref="Html"/> and <see cref="Items"/> carry the small
    /// amount of inline markup the design uses (only &lt;strong&gt;) and are rendered as a
    /// MarkupString. That is safe here because every value is a compile-time constant in this
    /// file; once article bodies come from storage they must be sanitised server-side before
    /// being rendered the same way.
    /// </summary>
    public sealed record ArticleBlock(
        ArticleBlockKind Kind,
        string? Html = null,
        string[]? Items = null,
        string? ImageUrl = null,
        string? ImageAlt = null)
    {
        public static ArticleBlock P(string html) => new(ArticleBlockKind.Paragraph, html);

        public static ArticleBlock H2(string text) => new(ArticleBlockKind.Heading2, text);

        public static ArticleBlock H3(string text) => new(ArticleBlockKind.Heading3, text);

        public static ArticleBlock Ul(params string[] items) =>
            new(ArticleBlockKind.BulletList, Items: items);

        public static ArticleBlock Img(string url, string alt) =>
            new(ArticleBlockKind.Image, ImageUrl: url, ImageAlt: alt);
    }

    public sealed record ArticleComment(string Name, string Initials, string Ago, string Text);

    public sealed record Article(
        string Slug,
        string Title,
        string Category,
        DateOnly PublishedOn,
        string Summary,
        string ImageUrl,
        string ImageAlt,
        string Author,
        string HeroImageUrl,
        string HeroImageAlt,
        string[] Tags,
        ArticleBlock[] Blocks,
        ArticleComment[] Comments)
    {
        /// <summary>Route to this article's detail page.</summary>
        public string Url => $"/bai-viet/{Slug}";
    }

    public static readonly string[] ArticleCategories =
    [
        "Kiến thức pha chế", "Câu chuyện nông trại", "Xu hướng thế giới"
    ];

    public static readonly Article[] Articles =
    [
        new("nghe-thuat-chiet-xuat-espresso",
            "Nghệ thuật chiết xuất Espresso: Hiểu rõ về áp suất và nhiệt độ",
            "Kiến thức pha chế", new DateOnly(2024, 10, 15),
            "Khám phá cách những thay đổi nhỏ về áp suất và nhiệt độ có thể biến đổi hoàn toàn hương vị " +
            "của một ly Espresso chuyên nghiệp.",
            "/images/blog-1.jpg",
            "Bộ dụng cụ pha chế barista gồm tay cầm, cân điện tử và cối nén trên bàn gỗ mộc",
            Author: "Minh Tú",
            HeroImageUrl: "/images/article-1.jpg",
            HeroImageAlt: "Dòng espresso sánh đặc chảy từ tay cầm chuyên nghiệp xuống tách sứ trắng với " +
                          "lớp crema vàng nâu dày",
            Tags: ["Espresso", "Kỹ năng Barista", "Chiết xuất"],
            Blocks:
            [
                ArticleBlock.P(
                    "Espresso không chỉ là một loại cà phê; nó là một hành trình kỳ diệu của việc chiết " +
                    "xuất những tinh túy nhất từ hạt cà phê thông qua sự can thiệp chính xác của áp suất " +
                    "và nhiệt độ. Trong môi trường chuyên nghiệp, việc nắm bắt &quot;điểm ngọt&quot; " +
                    "(sweet spot) của quá trình chiết xuất là kỹ năng phân biệt một barista bình thường " +
                    "và một nghệ nhân."),
                ArticleBlock.H2("1. Vai trò của Áp Suất (Pressure)"),
                ArticleBlock.P(
                    "Áp suất tiêu chuẩn để chiết xuất một ly espresso hoàn hảo thường được cố định ở mức " +
                    "9 bar. Tại sao lại là 9 bar? Mức áp suất này cung cấp đủ lực để ép nước nóng xuyên " +
                    "qua lớp cà phê được nén chặt, nhũ hóa các loại dầu tự nhiên trong cà phê để tạo ra " +
                    "lớp crema vàng óng đặc trưng."),
                ArticleBlock.Ul(
                    "<strong>Dưới 9 bar:</strong> Chiết xuất kém, cà phê có vị chua gắt, loãng và thiếu " +
                    "thể chất.",
                    "<strong>Trên 9 bar:</strong> Dễ dẫn đến hiện tượng channelling (nước tìm đường đi dễ " +
                    "nhất qua bánh cà phê), gây ra chiết xuất không đều, vị đắng chát."),
                ArticleBlock.H3("Hiện tượng Pre-infusion (Ủ cà phê)"),
                ArticleBlock.P(
                    "Trước khi áp dụng toàn bộ 9 bar, một giai đoạn pre-infusion với áp suất thấp " +
                    "(khoảng 2-3 bar) trong vài giây đầu giúp lớp cà phê nở đều, giảm thiểu rủi ro " +
                    "channelling và mang lại hương vị tròn trịa hơn."),
                ArticleBlock.Img("/images/article-2.jpg",
                    "Cận cảnh đồng hồ đo áp suất của máy espresso đang chỉ đúng 9 bar"),
                ArticleBlock.H2("2. Tầm quan trọng của Nhiệt Độ (Temperature)"),
                ArticleBlock.P(
                    "Nhiệt độ nước quyết định trực tiếp đến những hợp chất nào sẽ được hòa tan vào trong " +
                    "ly cà phê của bạn. Phạm vi nhiệt độ lý tưởng thường nằm trong khoảng 90°C đến 96°C."),
                ArticleBlock.P("Quy tắc chung khi điều chỉnh nhiệt độ:"),
                ArticleBlock.Ul(
                    "<strong>Nhiệt độ cao (94-96°C):</strong> Thích hợp cho cà phê rang nhạt (light " +
                    "roast) để tăng cường chiết xuất, giúp làm nổi bật hương hoa quả và vị chua sáng. " +
                    "Tuy nhiên, nếu dùng cho cà phê rang đậm sẽ dễ gây ra vị đắng khét.",
                    "<strong>Nhiệt độ thấp (90-92°C):</strong> Phù hợp với cà phê rang đậm (dark roast) " +
                    "để hạn chế vị đắng, làm nổi bật vị ngọt chocolate, caramel và tạo cảm giác êm ái hơn."),
                ArticleBlock.P(
                    "Tại <strong>Nie Farm Việt Nam</strong>, chúng tôi luôn khuyến khích các đối tác và " +
                    "khách hàng hiểu rõ đặc tính của từng mẻ rang để có những điều chỉnh áp suất và nhiệt " +
                    "độ phù hợp nhất, nhằm tôn vinh trọn vẹn công sức của người nông dân.")
            ],
            Comments:
            [
                new("Hoàng Trần", "HT", "2 ngày trước",
                    "Bài viết rất chi tiết. Mình thường gặp vấn đề vị chua gắt khi dùng light roast, nay " +
                    "mới hiểu rõ vai trò của việc tăng nhiệt độ lên một chút. Cảm ơn tác giả!")
            ]),

        new("hanh-trinh-hat-ca-phe",
            "Hành trình hạt cà phê: Từ cao nguyên mù sương đến tách cà phê hoàn hảo",
            "Câu chuyện nông trại", new DateOnly(2024, 10, 10),
            "Theo chân những người nông dân tận tụy trong hành trình thu hoạch và sơ chế những hạt cà phê " +
            "nhân xanh chất lượng cao nhất.",
            "/images/blog-2.jpg",
            "Đồi cà phê bậc thang trong sương sớm với ánh nắng vàng xuyên qua màn mù",
            Author: "Minh Tú",
            HeroImageUrl: "/images/blog-2.jpg",
            HeroImageAlt: "Đồi cà phê bậc thang trong sương sớm với ánh nắng vàng xuyên qua màn mù",
            Tags: ["Nông trại", "Sơ chế", "Nhân xanh"],
            Blocks: [],
            Comments: []),

        new("lan-song-ca-phe-thu-tu",
            "Làn sóng cà phê thứ 4: Khi khoa học và nghệ thuật hòa quyện",
            "Xu hướng thế giới", new DateOnly(2024, 10, 5),
            "Tìm hiểu về làn sóng mới nhất trong ngành công nghiệp cà phê, nơi dữ liệu, độ tinh khiết và " +
            "trải nghiệm cá nhân hóa lên ngôi.",
            "/images/blog-3.jpg",
            "Không gian quán cà phê hiện đại với máy espresso tối giản và quầy gỗ sồi sáng màu",
            Author: "Minh Tú",
            HeroImageUrl: "/images/blog-3.jpg",
            HeroImageAlt: "Không gian quán cà phê hiện đại với máy espresso tối giản và quầy gỗ sồi sáng màu",
            Tags: ["Xu hướng", "Specialty"],
            Blocks: [],
            Comments: []),

        // The three titles the design lists under "Bài viết liên quan". They share the
        // "Kiến thức pha chế" category, which is how RelatedArticles derives that sidebar.
        new("hanh-trinh-bien-doi-huong-vi-qua-cac-cap-do-rang",
            "Hành trình biến đổi hương vị qua các cấp độ rang",
            "Kiến thức pha chế", new DateOnly(2024, 10, 10),
            "Từ hạt nhân xanh đến mẻ rang đậm, mỗi cấp độ rang mở ra một dải hương vị hoàn toàn khác nhau.",
            "/images/article-3.jpg",
            "Các giai đoạn rang cà phê xếp cạnh nhau, từ hạt nhân xanh đến hạt nâu đậm trên nền ngà sáng",
            Author: "Minh Tú",
            HeroImageUrl: "/images/article-3.jpg",
            HeroImageAlt: "Các giai đoạn rang cà phê xếp cạnh nhau, từ hạt nhân xanh đến hạt nâu đậm",
            Tags: ["Rang xay", "Hương vị"],
            Blocks: [],
            Comments: []),

        new("bi-quyet-steam-sua",
            "Bí quyết Steam Sữa: Nhiệt độ và kết cấu hoàn hảo",
            "Kiến thức pha chế", new DateOnly(2024, 10, 5),
            "Kiểm soát nhiệt độ và độ mịn của bọt sữa là bước quyết định giữa một ly cappuccino đạt chuẩn " +
            "và một ly chỉ tạm ổn.",
            "/images/article-4.jpg",
            "Barista rót latte art hình rosetta trong không gian quán cà phê ấm cúng",
            Author: "Minh Tú",
            HeroImageUrl: "/images/article-4.jpg",
            HeroImageAlt: "Barista rót latte art hình rosetta trong không gian quán cà phê ấm cúng",
            Tags: ["Kỹ năng Barista", "Latte Art"],
            Blocks: [],
            Comments: []),

        new("pour-over-co-ban",
            "Pour Over cơ bản: Kiểm soát thời gian và dòng chảy",
            "Kiến thức pha chế", new DateOnly(2024, 9, 28),
            "Nắm vững nhịp rót và tổng thời gian chiết xuất để có một ly pour over trong trẻo, cân bằng.",
            "/images/article-5.jpg",
            "Bình Chemex thủy tinh đang nhỏ giọt cà phê lọc màu hổ phách dưới nắng sớm",
            Author: "Minh Tú",
            HeroImageUrl: "/images/article-5.jpg",
            HeroImageAlt: "Bình Chemex thủy tinh đang nhỏ giọt cà phê lọc màu hổ phách dưới nắng sớm",
            Tags: ["Pour Over", "Chiết xuất"],
            Blocks: [],
            Comments: [])
    ];

    /// <summary>Same-category articles, most recent first — backs the detail page sidebar.</summary>
    public static IEnumerable<Article> RelatedArticles(Article article, int take = 3) =>
        Articles
            .Where(a => a.Slug != article.Slug && a.Category == article.Category)
            .OrderByDescending(a => a.PublishedOn)
            .Take(take);

    /// <summary>
    /// The two products promoted in the article sidebar. A property rather than a field:
    /// static field initialisation order across partial-class files is not guaranteed, so
    /// reading <see cref="FeaturedProducts"/> eagerly here could observe it unset.
    /// </summary>
    public static ProductCard[] ArticleSidebarProducts =>
    [
        FeaturedProducts[1],
        FeaturedProducts[0]
    ];
}
