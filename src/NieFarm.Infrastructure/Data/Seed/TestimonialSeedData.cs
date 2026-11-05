using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Seed;

/// <summary>
/// The three quotes from the original Stitch-design fixture
/// (<c>NieFarm.Web/Content/SiteContent.cs</c>), transcribed here because Infrastructure cannot
/// reference Web. Used once by <see cref="Seeder"/> to populate an empty Testimonials table so a
/// freshly migrated site is not blank. No photo — the seeded caption is name and role only.
/// </summary>
public static class TestimonialSeedData
{
    public static IReadOnlyList<Testimonial> Build() =>
    [
        Testimonial.Create(
            quote: "Cà phê của Nie Farm thực sự khác biệt, hương vị đậm đà và quy trình minh bạch khiến tôi rất " +
                   "yên tâm khi sử dụng cho quán của mình.",
            authorName: "Nguyễn Văn A",
            authorRole: "Chủ chuỗi Cafe",
            photoUrl: string.Empty,
            rating: 5,
            sortOrder: 0,
            isVisible: true),

        Testimonial.Create(
            quote: "Tôi đánh giá cao sự chuyên nghiệp trong cách Nie Farm hỗ trợ kỹ thuật sơ chế. Chất lượng hạt " +
                   "nhân xanh cải thiện rõ rệt qua từng mùa vụ.",
            authorName: "Trần Thị B",
            authorRole: "Chủ trang trại cà phê",
            photoUrl: string.Empty,
            rating: 5,
            sortOrder: 1,
            isVisible: true),

        Testimonial.Create(
            quote: "Dịch vụ tận tâm và sản phẩm chất lượng. Nie Farm không chỉ bán cà phê, họ bán cả niềm tin và " +
                   "sự tử tế trong từng hạt đậu.",
            authorName: "Lê Văn C",
            authorRole: "Khách hàng cá nhân",
            photoUrl: string.Empty,
            rating: 5,
            sortOrder: 2,
            isVisible: true)
    ];
}
