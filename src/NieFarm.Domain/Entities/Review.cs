using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// A customer's star rating + comment on a <see cref="Product"/>. Invisible to everyone except
/// its own author until an admin approves it (<see cref="Approve"/>) — see
/// docs/plans/product-reviews.md for the full design. Carries a <see cref="ProductId"/> FK plus a
/// one-directional <see cref="Product"/> navigation; <see cref="Product"/> has no Reviews
/// collection, so this aggregate is queried standalone, like <see cref="Cart"/>.
/// </summary>
public class Review : BaseEntity
{
    public const int CommentMinLength = 10;
    public const int CommentMaxLength = 2000;

    public int ProductId { get; private set; }
    public Product? Product { get; private set; }

    /// <summary>Identity user id of the author.</summary>
    public string AuthorUserId { get; private set; } = string.Empty;

    /// <summary>Display-name snapshot taken at submit time.</summary>
    public string AuthorName { get; private set; } = string.Empty;

    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending;

    public DateTime? ModeratedAt { get; private set; }
    public string? ModeratedByUserId { get; private set; }

    /// <summary>Set only by <see cref="EditComment"/> — an admin's copy-edit, never the author's.</summary>
    public DateTime? CommentEditedAt { get; private set; }
    public string? CommentEditedByUserId { get; private set; }

    /// <summary>A review's content is editable by its own author only while it is still Pending.</summary>
    public bool IsEditableByAuthor => Status == ReviewStatus.Pending;

    private Review() { }

    public static Review Create(int productId, string authorUserId, string authorName, int rating, string comment)
    {
        if (string.IsNullOrWhiteSpace(authorUserId))
            throw new InvalidOperationException("Review author is required.");

        var review = new Review
        {
            ProductId = productId,
            AuthorUserId = authorUserId,
            AuthorName = authorName.Trim(),
            Status = ReviewStatus.Pending
        };
        review.SetContent(rating, comment);
        return review;
    }

    /// <summary>
    /// Author-initiated edit of the rating/comment pair. Only legal while
    /// <see cref="IsEditableByAuthor"/> — throws otherwise as the last line of defence; the
    /// handler is expected to check <see cref="IsEditableByAuthor"/> first and return a friendly
    /// result instead of letting the customer see this exception.
    /// </summary>
    public void Edit(int rating, string comment)
    {
        if (!IsEditableByAuthor)
            throw new InvalidOperationException("A moderated review can no longer be edited by its author.");

        SetContent(rating, comment);
    }

    /// <summary>
    /// Admin-only copy-edit of the comment text. Never touches <see cref="Rating"/> (rewriting
    /// the rating is a different, out-of-scope feature), <see cref="Status"/>,
    /// <see cref="ModeratedAt"/> or <see cref="ModeratedByUserId"/> — legal in any status, since
    /// the moderator is the vetting authority here, not a party being bypassed.
    /// </summary>
    public void EditComment(string comment, string editorUserId)
    {
        Comment = NormalizeComment(comment);
        CommentEditedAt = DateTime.UtcNow;
        CommentEditedByUserId = editorUserId;
        SetUpdatedAt();
    }

    public void Approve(string moderatorUserId)
    {
        Status = ReviewStatus.Approved;
        ModeratedAt = DateTime.UtcNow;
        ModeratedByUserId = moderatorUserId;
        SetUpdatedAt();
    }

    public void Reject(string moderatorUserId)
    {
        Status = ReviewStatus.Rejected;
        ModeratedAt = DateTime.UtcNow;
        ModeratedByUserId = moderatorUserId;
        SetUpdatedAt();
    }

    /// <summary>The single home for the rating/comment guards shared by <see cref="Create"/> and <see cref="Edit"/>.</summary>
    private void SetContent(int rating, string comment)
    {
        if (rating is < 1 or > 5)
            throw new InvalidOperationException("Review rating must be between 1 and 5.");

        Rating = rating;
        Comment = NormalizeComment(comment);
        SetUpdatedAt();
    }

    /// <summary>
    /// The single definition of the comment rules, shared by <see cref="SetContent"/> (customer
    /// path) and <see cref="EditComment"/> (admin path) — deliberately not sharing
    /// <see cref="SetContent"/> itself, since an admin comment edit has no rating to pass.
    /// </summary>
    private static string NormalizeComment(string comment)
    {
        var trimmed = (comment ?? string.Empty).Trim();
        if (trimmed.Length is < CommentMinLength or > CommentMaxLength)
            throw new InvalidOperationException(
                $"Review comment must be between {CommentMinLength} and {CommentMaxLength} characters.");

        return trimmed;
    }
}
