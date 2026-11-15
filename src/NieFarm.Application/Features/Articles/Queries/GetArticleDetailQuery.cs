using MediatR;
using NieFarm.Application.Features.Articles.Dtos;

namespace NieFarm.Application.Features.Articles.Queries;

/// <summary>Null when the slug is unknown, unpublished or in a hidden category (a real 404).</summary>
public record GetArticleDetailQuery(string Slug) : IRequest<ArticleDetailDto?>;
