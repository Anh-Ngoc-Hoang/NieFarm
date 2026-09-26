using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Infrastructure.Data.Seed;

/// <summary>
/// The six activities from the original Stitch-design fixture
/// (<c>NieFarm.Web/Content/SiteContent.cs</c>), transcribed here because Infrastructure cannot
/// reference Web. Used once by <see cref="Seeder"/> to populate an empty Activities table so a
/// freshly migrated site is not blank. The image URLs deliberately stay as the top-level
/// <c>/images/activities-N.jpg</c> paths (not moved under the "activities" upload subfolder) so
/// they remain outside <c>ManagedImages.IsManagedImage</c> and can never be deleted by an admin
/// action. Bodies are wrapped as HTML paragraphs since <see cref="Activity.Body"/> is now rendered
/// via the admin rich-text editor's output, not plain text.
/// </summary>
public static class ActivitySeedData
{
    private static string AsHtmlParagraphs(string[] paragraphs) =>
        string.Concat(paragraphs.Select(p => $"<p>{p}</p>"));

    public static IReadOnlyList<Activity> Build() =>
    [
        Activity.Create(
            title: "Mùa Thu Hoạch 2024",
            slug: "thu-hoach-2024",
            modalTitle: "Mùa Thu Hoạch 2024: Năng Suất Vượt Trội",
            eventDate: new DateOnly(2024, 11, 15),
            imageUrl: "/images/activities-1.jpg",
            imageAlt: "Nông dân thu hái quả cà phê chín đỏ trên nông trại ngập nắng",
            cardSize: ActivityCardSize.Tall,
            body: AsHtmlParagraphs(
            [
                "Mùa thu hoạch năm nay tại Nie Farm đánh dấu một bước ngoặt quan trọng trong việc áp dụng " +
                "các quy chuẩn kỹ thuật mới. Nhờ thời tiết thuận lợi và quy trình chăm sóc tỉ mỉ, tỷ lệ " +
                "quả chín đồng đều đạt mức kỷ lục.",
                "Sự kiện thu hoạch không chỉ là công việc, mà còn là ngày hội của cả cộng đồng nông dân " +
                "địa phương. Chúng tôi cam kết duy trì phương pháp hái chọn lọc bằng tay 100% để đảm bảo " +
                "chỉ những quả cà phê chín mọng nhất mới được đưa vào quy trình chế biến tiếp theo.",
                "Mỗi hạt cà phê đều mang trong mình câu chuyện về sự tận tâm và cam kết chất lượng tuyệt " +
                "đối từ Nie Farm."
            ]),
            isPublished: true),

        Activity.Create(
            title: "Kiểm Định Chất Lượng Quả Tươi",
            slug: "kiem-dinh-qua-tuoi",
            modalTitle: "Kiểm Định Chất Lượng Quả Tươi",
            eventDate: new DateOnly(2024, 10, 20),
            imageUrl: "/images/activities-2.jpg",
            imageAlt: "Quả cà phê chín đỏ vừa thu hoạch trong giỏ đan đặt trên bàn gỗ sạch sẽ",
            cardSize: ActivityCardSize.Short,
            body: AsHtmlParagraphs(
            [
                "Mỗi lô quả tươi về xưởng đều được lấy mẫu ngẫu nhiên để đo độ Brix, tỷ lệ quả chín và tỷ " +
                "lệ tạp chất trước khi bước vào công đoạn sơ chế.",
                "Chỉ những lô đạt ngưỡng chất lượng nội bộ mới được giữ lại cho dòng sản phẩm Specialty; " +
                "phần còn lại được tách riêng để đảm bảo tính nhất quán của từng mẻ rang."
            ]),
            isPublished: true),

        Activity.Create(
            title: "Hội Thảo Canh Tác Bền Vững",
            slug: "hoi-thao-canh-tac",
            modalTitle: "Hội Thảo Canh Tác Bền Vững",
            eventDate: new DateOnly(2024, 9, 12),
            imageUrl: "/images/activities-3.jpg",
            imageAlt: "Nông dân và chuyên gia nông nghiệp cùng phân tích mẫu đất trong buổi hội thảo",
            cardSize: ActivityCardSize.ExtraTall,
            body: AsHtmlParagraphs(
            [
                "Nie Farm phối hợp cùng các chuyên gia nông nghiệp tổ chức hội thảo hướng dẫn bà con phân " +
                "tích mẫu đất, cân đối dinh dưỡng và quản lý nguồn nước tưới.",
                "Mục tiêu dài hạn là giảm phụ thuộc vào phân bón hóa học, đồng thời giữ ổn định năng suất " +
                "qua từng mùa vụ."
            ]),
            isPublished: true),

        Activity.Create(
            title: "Quy Trình Rang Thử Nghiệm",
            slug: "rang-thu-nghiem",
            modalTitle: "Quy Trình Rang Thử Nghiệm",
            eventDate: new DateOnly(2024, 8, 5),
            imageUrl: "/images/activities-4.jpg",
            imageAlt: "Máy rang cà phê trong xưởng hiện đại với ánh sáng ấm màu đồng",
            cardSize: ActivityCardSize.Medium,
            body: AsHtmlParagraphs(
            [
                "Mỗi giống cà phê mới đều trải qua nhiều mẻ rang thử với các hồ sơ nhiệt khác nhau, sau đó " +
                "được cupping mù để chọn ra hồ sơ tối ưu.",
                "Toàn bộ dữ liệu đường cong nhiệt được lưu lại, giúp tái lập chính xác chất lượng ở quy mô " +
                "sản xuất."
            ]),
            isPublished: true),

        Activity.Create(
            title: "Chân Dung Người Nông Dân",
            slug: "chan-dung-nong-dan",
            modalTitle: "Chân Dung Người Nông Dân",
            eventDate: new DateOnly(2024, 7, 18),
            imageUrl: "/images/activities-5.jpg",
            imageAlt: "Chân dung người nông dân trồng cà phê đứng giữa vườn cây được chăm sóc kỹ lưỡng",
            cardSize: ActivityCardSize.Tall,
            body: AsHtmlParagraphs(
            [
                "Đằng sau mỗi hạt cà phê là những người nông dân gắn bó với mảnh đất Tây Nguyên qua nhiều " +
                "thế hệ.",
                "Nie Farm cam kết thu mua minh bạch, chia sẻ kỹ thuật và đồng hành cùng bà con trong việc " +
                "nâng cao giá trị nông sản."
            ]),
            isPublished: true),

        Activity.Create(
            title: "Nâng Cấp Cơ Sở Chế Biến",
            slug: "nang-cap-co-so",
            modalTitle: "Nâng Cấp Cơ Sở Chế Biến",
            eventDate: new DateOnly(2024, 6, 2),
            imageUrl: "/images/activities-6.jpg",
            imageAlt: "Toàn cảnh khu chế biến nông sản hiện đại với đường nét kiến trúc tối giản",
            cardSize: ActivityCardSize.Short,
            body: AsHtmlParagraphs(
            [
                "Khu sơ chế được mở rộng với hệ thống nhà màng khép kín và giàn phơi cách đất, tăng gấp " +
                "đôi công suất xử lý trong mùa cao điểm.",
                "Cải tiến này rút ngắn thời gian từ lúc hái đến lúc đưa vào sơ chế, giữ trọn hương vị " +
                "nguyên bản."
            ]),
            isPublished: true)
    ];
}
