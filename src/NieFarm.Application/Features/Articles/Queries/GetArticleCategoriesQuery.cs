using MediatR;
using NieFarm.Application.Features.Articles.Dtos;

namespace NieFarm.Application.Features.Articles.Queries;

/// <summary>
/// Visible article categories for the public /kien-thuc filter. The admin feature has a query of
/// the same simple name in <c>Features.Admin.ArticleCategories.Queries</c>; mind the namespace.
/// </summary>
public record GetArticleCategoriesQuery : IRequest<IReadOnlyList<ArticleCategoryFilterDto>>;
