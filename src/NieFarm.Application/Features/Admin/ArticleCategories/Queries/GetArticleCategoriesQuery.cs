using MediatR;
using NieFarm.Application.Features.Admin.ArticleCategories.Dtos;

namespace NieFarm.Application.Features.Admin.ArticleCategories.Queries;

public record GetArticleCategoriesQuery : IRequest<List<ArticleCategoryDto>>;
