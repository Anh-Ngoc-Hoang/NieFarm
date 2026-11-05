using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

/// <summary>
/// A customer quote shown in the home page's testimonial carousel. Standalone — no FK to any
/// other aggregate, moderated only through admin CRUD at /admin/testimonials (no author-facing
/// submission flow, unlike <see cref="Review"/>).
/// </summary>
public class Testimonial : BaseEntity
{
    public const int QuoteMaxLength = 600;
    public const int AuthorNameMaxLength = 100;
    public const int AuthorRoleMaxLength = 100;
    public const int PhotoUrlMaxLength = 500;

    public string Quote { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public string AuthorRole { get; private set; } = string.Empty;

    /// <summary>Optional and decorative — the author's name is rendered right beside it, so there is no alt column.</summary>
    public string PhotoUrl { get; private set; } = string.Empty;

    /// <summary>1–5, default 5.</summary>
    public int Rating { get; private set; } = 5;

    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; } = true;

    private Testimonial() { }

    public static Testimonial Create(
        string quote, string authorName, string authorRole, string photoUrl,
        int rating, int sortOrder, bool isVisible)
    {
        var testimonial = new Testimonial();
        testimonial.Update(quote, authorName, authorRole, photoUrl, rating, sortOrder, isVisible);
        return testimonial;
    }

    /// <summary>
    /// The validator enforces 1..5 for a friendly message; this is the last line of defence
    /// against a malformed rating actually being persisted.
    /// </summary>
    public void Update(
        string quote, string authorName, string authorRole, string photoUrl,
        int rating, int sortOrder, bool isVisible)
    {
        if (rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");

        Quote = quote.Trim();
        AuthorName = authorName.Trim();
        AuthorRole = authorRole.Trim();
        PhotoUrl = photoUrl.Trim();
        Rating = rating;
        SortOrder = sortOrder;
        IsVisible = isVisible;
        SetUpdatedAt();
    }
}
