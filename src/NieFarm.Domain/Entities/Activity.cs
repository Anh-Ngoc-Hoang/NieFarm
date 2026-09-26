using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

public class Activity : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Heading shown in the detail modal; blank falls back to <see cref="Title"/>.</summary>
    public string ModalTitle { get; private set; } = string.Empty;

    public DateOnly EventDate { get; private set; }

    /// <summary>Single image used for both the card and the modal.</summary>
    public string ImageUrl { get; private set; } = string.Empty;
    public string ImageAlt { get; private set; } = string.Empty;

    public ActivityCardSize CardSize { get; private set; }

    /// <summary>Plain text; paragraphs are separated by a blank line.</summary>
    public string Body { get; private set; } = string.Empty;

    public bool IsPublished { get; private set; } = true;

    private Activity() { }

    public static Activity Create(
        string title, string slug, string modalTitle, DateOnly eventDate,
        string imageUrl, string imageAlt, ActivityCardSize cardSize, string body, bool isPublished)
    {
        var activity = new Activity();
        activity.Update(title, slug, modalTitle, eventDate, imageUrl, imageAlt, cardSize, body, isPublished);
        return activity;
    }

    public void Update(
        string title, string slug, string modalTitle, DateOnly eventDate,
        string imageUrl, string imageAlt, ActivityCardSize cardSize, string body, bool isPublished)
    {
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ModalTitle = modalTitle.Trim();
        EventDate = eventDate;
        ImageUrl = imageUrl.Trim();
        ImageAlt = imageAlt.Trim();
        CardSize = cardSize;
        Body = body;
        IsPublished = isPublished;
        SetUpdatedAt();
    }
}
