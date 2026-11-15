namespace NieFarm.Application.Features.Articles.Dtos;

/// <summary>
/// Public article detail. <see cref="BodyHtml"/> is sanitised server-side on every read, so it is
/// safe to render through <c>MarkupString</c>. <see cref="HeroImageUrl"/> is null when the article
/// has no image at all.
/// </summary>
public record ArticleDetailDto(
    int Id,
    string Slug,
    string Title,
    string CategoryName,
    string CategorySlug,
    string Author,
    DateOnly PublishedOn,
    string Summary,
    string? HeroImageUrl,
    string HeroImageAlt,
    string BodyHtml,
    IReadOnlyList<ArticleCardDto> Related);
