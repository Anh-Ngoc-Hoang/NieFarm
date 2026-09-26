using MediatR;
using NieFarm.Application.Features.Admin.Articles.Dtos;

namespace NieFarm.Application.Features.Admin.Articles.Queries;

public record GetArticlesAdminQuery : IRequest<List<ArticleAdminListDto>>;
