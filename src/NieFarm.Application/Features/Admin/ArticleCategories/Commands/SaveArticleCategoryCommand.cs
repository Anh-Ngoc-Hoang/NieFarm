using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.ArticleCategories.Commands;

/// <summary>Creates when <paramref name="Id"/> is null, otherwise updates that row.</summary>
public record SaveArticleCategoryCommand(
    int? Id,
    string Name,
    string? Slug,
    bool IsVisible,
    int SortOrder) : IRequest<Result<int>>;
