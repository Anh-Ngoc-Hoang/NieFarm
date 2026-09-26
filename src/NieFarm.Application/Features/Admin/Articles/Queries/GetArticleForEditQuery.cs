using MediatR;
using NieFarm.Application.Features.Admin.Articles.Dtos;

namespace NieFarm.Application.Features.Admin.Articles.Queries;

public record GetArticleForEditQuery(int Id) : IRequest<ArticleEditDto?>;
