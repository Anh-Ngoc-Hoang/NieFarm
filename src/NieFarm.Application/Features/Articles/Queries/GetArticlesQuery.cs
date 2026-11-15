using MediatR;
using NieFarm.Application.Features.Articles.Dtos;

namespace NieFarm.Application.Features.Articles.Queries;

/// <summary>Public /kien-thuc listing. <c>CategoryId</c> filters by Id, never slug.</summary>
public record GetArticlesQuery(int? CategoryId = null, string? Search = null)
    : IRequest<IReadOnlyList<ArticleCardDto>>;
