using NieFarm.Domain.Entities;

namespace NieFarm.Infrastructure.Data.Seed;

/// <summary>
/// The 2027 "New Crop" placeholder seeded so the home page's New Crop section is not blank on a
/// fresh install. Content is placeholder marketing copy consistent with /quy-trinh — farm
/// confirmation of the real region/altitude/varieties/process/tasting notes is an open item (see
/// docs/plans/home-redesign-crop-testimonials.md §7). The seeded image stays at the top-level
/// /images/quy-trinh-4.jpg path (already used by /quy-trinh's own stage 4), not under the "crops"
/// upload subfolder, so it is never admin-deletable.
/// </summary>
public static class CropSeedData
{
    public static Crop Build(IReadOnlyDictionary<string, int> productIdBySlug)
    {
        var crop = Crop.Create(
            year: 2027,
            seasonLabel: "Tháng 11 – Tháng 1",
            headline: "Mùa quả chín đỏ trên cao nguyên 1.450m",
            story:
                "Hái tay, chọn từng quả chín 100%, quay lại cùng một cây nhiều lượt trong mùa thay vì " +
                "tuốt một lần — nền tảng của toàn bộ hương vị phía sau.\n\n" +
                "Sơ chế Honey và Washed trong vòng 12 giờ sau khi hái, rồi phơi trên giàn cách đất " +
                "trong nhà màng, đảo tay nhiều lần mỗi ngày, dừng đúng ở độ ẩm 12%.",
            region: "Cầu Đất, Lâm Đồng",
            altitude: "1.450m",
            varieties: "Arabica Typica, Robusta chọn lọc",
            process: "Honey & Washed",
            tastingNotes: "Mật ong, Sô-cô-la đen, Cam chín, Hạnh nhân rang",
            imageUrl: "/images/quy-trinh-4.jpg",
            imageAlt: "Đôi tay đeo găng chọn hái những quả cà phê chín đỏ vào gùi tre trong vườn hữu cơ");

        string[] shopSlugs = ["robusta-honey-dac-biet", "arabica-cau-dat"];
        var productIds = shopSlugs
            .Where(productIdBySlug.ContainsKey)
            .Select(slug => productIdBySlug[slug])
            .ToList();

        crop.SetProducts(productIds);
        crop.MarkCurrent();

        return crop;
    }
}
