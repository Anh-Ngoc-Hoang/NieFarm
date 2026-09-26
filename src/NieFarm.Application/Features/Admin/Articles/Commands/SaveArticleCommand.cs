using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Articles.Commands;

/// <summary>
/// Creates when <paramref name="Id"/> is null, otherwise updates that row.
/// A blank <paramref name="Slug"/> is generated from the title.
/// </summary>
public record SaveArticleCommand(
    int? Id,
    string Title,
    string? Slug,
    int CategoryId,
    string Author,
    string Summary,
    string ImageUrl,
    string ImageAlt,
    string HeroImageUrl,
    string HeroImageAlt,
    string Body,
    DateOnly PublishedOn,
    bool IsPublished) : IRequest<Result<int>>;
