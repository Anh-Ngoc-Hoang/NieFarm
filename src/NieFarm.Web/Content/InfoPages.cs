namespace NieFarm.Web.Content;

/// <summary>Which footer / side-nav group an info page belongs to.</summary>
public enum InfoPageGroup { Policy, Partnership }

/// <summary>
/// One static info page. <paramref name="Title"/> is the full name (h1 and &lt;title&gt;);
/// <paramref name="NavLabel"/> is the short name used in the footer and side nav.
/// </summary>
public sealed record InfoPageLink(string Href, string Title, string NavLabel, InfoPageGroup Group);

/// <summary>
/// Presentation registry for the static policy / careers / partnership pages (R-05).
/// The footer, the page side nav, breadcrumbs and each page's h1 and title all read from here.
/// </summary>
public static class InfoPages
{
    public const string Terms = "/dieu-khoan-su-dung";
    public const string Privacy = "/chinh-sach-bao-mat";
    public const string Payment = "/chinh-sach-thanh-toan";
    public const string Shipping = "/chinh-sach-van-chuyen";
    public const string Returns = "/chinh-sach-doi-tra";
    public const string Careers = "/tuyen-dung";
    public const string Partnership = "/hop-tac-dai-ly";

    /// <summary>Every info page, in display order.</summary>
    public static IReadOnlyList<InfoPageLink> All { get; } =
    [
        new(Terms, "Điều khoản sử dụng", "Điều khoản sử dụng", InfoPageGroup.Policy),
        new(Privacy, "Chính sách bảo mật", "Bảo mật", InfoPageGroup.Policy),
        new(Payment, "Chính sách thanh toán", "Thanh toán", InfoPageGroup.Policy),
        new(Shipping, "Chính sách vận chuyển", "Vận chuyển", InfoPageGroup.Policy),
        new(Returns, "Chính sách đổi trả", "Đổi trả", InfoPageGroup.Policy),
        new(Careers, "Tuyển dụng", "Tuyển dụng", InfoPageGroup.Partnership),
        new(Partnership, "Hợp tác đại lý", "Hợp tác đại lý", InfoPageGroup.Partnership),
    ];

    public static IEnumerable<InfoPageLink> InGroup(InfoPageGroup group) =>
        All.Where(p => p.Group == group);

    /// <summary>Resolves a page by route. Throws for an unknown route: that is a programming error.</summary>
    public static InfoPageLink Get(string href) =>
        All.FirstOrDefault(p => p.Href == href)
        ?? throw new InvalidOperationException($"No info page is registered for '{href}'.");

    public static string GroupLabel(InfoPageGroup group) => group switch
    {
        InfoPageGroup.Policy => "Chính sách",
        _ => "Hợp tác",
    };

    /// <summary>
    /// Link to the footer's contact anchor from <paramref name="href"/>. The document has
    /// <c>&lt;base href="/"&gt;</c>, so a bare <c>#lien-he</c> resolves to <c>/#lien-he</c> and
    /// navigates to the home page; the fragment must carry the page's own path.
    /// </summary>
    public static string ContactHref(string href) => href + "#lien-he";
}
