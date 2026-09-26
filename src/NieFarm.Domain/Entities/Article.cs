using NieFarm.Domain.Common;

namespace NieFarm.Domain.Entities;

public class Article : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public ArticleCategory? Category { get; private set; }

    public string Author { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;

    /// <summary>Card thumbnail used on the /kien-thuc listing.</summary>
    public string ImageUrl { get; private set; } = string.Empty;
    public string ImageAlt { get; private set; } = string.Empty;

    /// <summary>Wide image at the top of the detail page.</summary>
    public string HeroImageUrl { get; private set; } = string.Empty;
    public string HeroImageAlt { get; private set; } = string.Empty;

    /// <summary>
    /// Article body as HTML produced by the admin editor. Sanitise before rendering:
    /// see the note in CLAUDE.md about MarkupString.
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    public DateOnly PublishedOn { get; private set; }
    public bool IsPublished { get; private set; } = true;

    private Article() { }

    public static Article Create(
        string title, string slug, int categoryId, string author, string summary,
        string imageUrl, string imageAlt, string heroImageUrl, string heroImageAlt,
        string body, DateOnly publishedOn, bool isPublished)
    {
        var article = new Article();
        article.Update(title, slug, categoryId, author, summary, imageUrl, imageAlt,
            heroImageUrl, heroImageAlt, body, publishedOn, isPublished);
        return article;
    }

    public void Update(
        string title, string slug, int categoryId, string author, string summary,
        string imageUrl, string imageAlt, string heroImageUrl, string heroImageAlt,
        string body, DateOnly publishedOn, bool isPublished)
    {
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        CategoryId = categoryId;
        Author = author.Trim();
        Summary = summary.Trim();
        ImageUrl = imageUrl.Trim();
        ImageAlt = imageAlt.Trim();
        HeroImageUrl = heroImageUrl.Trim();
        HeroImageAlt = heroImageAlt.Trim();
        Body = body;
        PublishedOn = publishedOn;
        IsPublished = isPublished;
        SetUpdatedAt();
    }
}
