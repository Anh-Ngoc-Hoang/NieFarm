using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.ArticleCategories.Commands;

public record DeleteArticleCategoryCommand(int Id) : IRequest<Result>;
