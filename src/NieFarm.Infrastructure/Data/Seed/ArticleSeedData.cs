using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Seed;

/// <summary>
/// The three article categories and six articles from the original Stitch-design fixture
/// (<c>NieFarm.Web/Content/SiteContent.Articles.cs</c>, since deleted), transcribed here because
/// Infrastructure cannot reference Web. Used by <see cref="Seeder"/> so a freshly migrated site is
/// not blank. Image URLs stay at the top-level <c>/images/…</c> paths so they remain outside
/// <c>ArticleImageLimits.IsManagedImage</c> and can never be deleted by an admin action. Category
/// slugs are written directly as ASCII (the generator is bypassed, as for products).
/// Only the first article has a body (already in the sanitised shape: p, h2, h3, ul/li, strong, no
/// img); the rest keep an empty body and the page shows its "being updated" fallback.
/// </summary>
public static class ArticleSeedData
{
    public static IReadOnlyList<ArticleCategory> BuildCategories() =>
    [
        ArticleCategory.Create("Kiến thức pha chế", "kien-thuc-pha-che", true, 1),
        ArticleCategory.Create("Câu chuyện nông trại", "cau-chuyen-nong-trai", true, 2),
        ArticleCategory.Create("Xu hướng thế giới", "xu-huong-the-gioi", true, 3)
    ];

    private const string EspressoBody =
        "<p>Espresso không chỉ là một loại cà phê; nó là một hành trình kỳ diệu của việc chiết " +
        "xuất những tinh túy nhất từ hạt cà phê thông qua sự can thiệp chính xác của áp suất " +
        "và nhiệt độ. Trong môi trường chuyên nghiệp, việc nắm bắt &quot;điểm ngọt&quot; " +
        "(sweet spot) của quá trình chiết xuất là kỹ năng phân biệt một barista bình thường " +
        "và một nghệ nhân.</p>" +
        "<h2>1. Vai trò của Áp Suất (Pressure)</h2>" +
        "<p>Áp suất tiêu chuẩn để chiết xuất một ly espresso hoàn hảo thường được cố định ở mức " +
        "9 bar. Tại sao lại là 9 bar? Mức áp suất này cung cấp đủ lực để ép nước nóng xuyên " +
        "qua lớp cà phê được nén chặt, nhũ hóa các loại dầu tự nhiên trong cà phê để tạo ra " +
        "lớp crema vàng óng đặc trưng.</p>" +
        "<ul>" +
        "<li><strong>Dưới 9 bar:</strong> Chiết xuất kém, cà phê có vị chua gắt, loãng và thiếu " +
        "thể chất.</li>" +
        "<li><strong>Trên 9 bar:</strong> Dễ dẫn đến hiện tượng channelling (nước tìm đường đi dễ " +
        "nhất qua bánh cà phê), gây ra chiết xuất không đều, vị đắng chát.</li>" +
        "</ul>" +
        "<h3>Hiện tượng Pre-infusion (Ủ cà phê)</h3>" +
        "<p>Trước khi áp dụng toàn bộ 9 bar, một giai đoạn pre-infusion với áp suất thấp " +
        "(khoảng 2-3 bar) trong vài giây đầu giúp lớp cà phê nở đều, giảm thiểu rủi ro " +
        "channelling và mang lại hương vị tròn trịa hơn.</p>" +
        "<h2>2. Tầm quan trọng của Nhiệt Độ (Temperature)</h2>" +
        "<p>Nhiệt độ nước quyết định trực tiếp đến những hợp chất nào sẽ được hòa tan vào trong " +
        "ly cà phê của bạn. Phạm vi nhiệt độ lý tưởng thường nằm trong khoảng 90°C đến 96°C.</p>" +
        "<p>Quy tắc chung khi điều chỉnh nhiệt độ:</p>" +
        "<ul>" +
        "<li><strong>Nhiệt độ cao (94-96°C):</strong> Thích hợp cho cà phê rang nhạt (light " +
        "roast) để tăng cường chiết xuất, giúp làm nổi bật hương hoa quả và vị chua sáng. " +
        "Tuy nhiên, nếu dùng cho cà phê rang đậm sẽ dễ gây ra vị đắng khét.</li>" +
        "<li><strong>Nhiệt độ thấp (90-92°C):</strong> Phù hợp với cà phê rang đậm (dark roast) " +
        "để hạn chế vị đắng, làm nổi bật vị ngọt chocolate, caramel và tạo cảm giác êm ái hơn.</li>" +
        "</ul>" +
        "<p>Tại <strong>Nie Farm Việt Nam</strong>, chúng tôi luôn khuyến khích các đối tác và " +
        "khách hàng hiểu rõ đặc tính của từng mẻ rang để có những điều chỉnh áp suất và nhiệt " +
        "độ phù hợp nhất, nhằm tôn vinh trọn vẹn công sức của người nông dân.</p>";

    /// <param name="categoryIdByName">Category Id keyed by Vietnamese name.</param>
    public static IReadOnlyList<Article> Build(IReadOnlyDictionary<string, int> categoryIdByName)
    {
        const string brew = "Kiến thức pha chế";
        const string farm = "Câu chuyện nông trại";
        const string trend = "Xu hướng thế giới";
        const string author = "Minh Tú";

        return
        [
            Article.Create(
                "Nghệ thuật chiết xuất Espresso: Hiểu rõ về áp suất và nhiệt độ",
                "nghe-thuat-chiet-xuat-espresso", categoryIdByName[brew], author,
                "Khám phá cách những thay đổi nhỏ về áp suất và nhiệt độ có thể biến đổi hoàn toàn hương vị " +
                "của một ly Espresso chuyên nghiệp.",
                "/images/blog-1.jpg",
                "Bộ dụng cụ pha chế barista gồm tay cầm, cân điện tử và cối nén trên bàn gỗ mộc",
                "/images/article-1.jpg",
                "Dòng espresso sánh đặc chảy từ tay cầm chuyên nghiệp xuống tách sứ trắng với " +
                "lớp crema vàng nâu dày",
                EspressoBody, new DateOnly(2024, 10, 15), true),

            Article.Create(
                "Hành trình hạt cà phê: Từ cao nguyên mù sương đến tách cà phê hoàn hảo",
                "hanh-trinh-hat-ca-phe", categoryIdByName[farm], author,
                "Theo chân những người nông dân tận tụy trong hành trình thu hoạch và sơ chế những hạt cà phê " +
                "nhân xanh chất lượng cao nhất.",
                "/images/blog-2.jpg",
                "Đồi cà phê bậc thang trong sương sớm với ánh nắng vàng xuyên qua màn mù",
                "/images/blog-2.jpg",
                "Đồi cà phê bậc thang trong sương sớm với ánh nắng vàng xuyên qua màn mù",
                string.Empty, new DateOnly(2024, 10, 10), true),

            Article.Create(
                "Làn sóng cà phê thứ 4: Khi khoa học và nghệ thuật hòa quyện",
                "lan-song-ca-phe-thu-tu", categoryIdByName[trend], author,
                "Tìm hiểu về làn sóng mới nhất trong ngành công nghiệp cà phê, nơi dữ liệu, độ tinh khiết và " +
                "trải nghiệm cá nhân hóa lên ngôi.",
                "/images/blog-3.jpg",
                "Không gian quán cà phê hiện đại với máy espresso tối giản và quầy gỗ sồi sáng màu",
                "/images/blog-3.jpg",
                "Không gian quán cà phê hiện đại với máy espresso tối giản và quầy gỗ sồi sáng màu",
                string.Empty, new DateOnly(2024, 10, 5), true),

            Article.Create(
                "Hành trình biến đổi hương vị qua các cấp độ rang",
                "hanh-trinh-bien-doi-huong-vi-qua-cac-cap-do-rang", categoryIdByName[brew], author,
                "Từ hạt nhân xanh đến mẻ rang đậm, mỗi cấp độ rang mở ra một dải hương vị hoàn toàn khác nhau.",
                "/images/article-3.jpg",
                "Các giai đoạn rang cà phê xếp cạnh nhau, từ hạt nhân xanh đến hạt nâu đậm trên nền ngà sáng",
                "/images/article-3.jpg",
                "Các giai đoạn rang cà phê xếp cạnh nhau, từ hạt nhân xanh đến hạt nâu đậm",
                string.Empty, new DateOnly(2024, 10, 10), true),

            Article.Create(
                "Bí quyết Steam Sữa: Nhiệt độ và kết cấu hoàn hảo",
                "bi-quyet-steam-sua", categoryIdByName[brew], author,
                "Kiểm soát nhiệt độ và độ mịn của bọt sữa là bước quyết định giữa một ly cappuccino đạt chuẩn " +
                "và một ly chỉ tạm ổn.",
                "/images/article-4.jpg",
                "Barista rót latte art hình rosetta trong không gian quán cà phê ấm cúng",
                "/images/article-4.jpg",
                "Barista rót latte art hình rosetta trong không gian quán cà phê ấm cúng",
                string.Empty, new DateOnly(2024, 10, 5), true),

            Article.Create(
                "Pour Over cơ bản: Kiểm soát thời gian và dòng chảy",
                "pour-over-co-ban", categoryIdByName[brew], author,
                "Nắm vững nhịp rót và tổng thời gian chiết xuất để có một ly pour over trong trẻo, cân bằng.",
                "/images/article-5.jpg",
                "Bình Chemex thủy tinh đang nhỏ giọt cà phê lọc màu hổ phách dưới nắng sớm",
                "/images/article-5.jpg",
                "Bình Chemex thủy tinh đang nhỏ giọt cà phê lọc màu hổ phách dưới nắng sớm",
                string.Empty, new DateOnly(2024, 9, 28), true)
        ];
    }
}
