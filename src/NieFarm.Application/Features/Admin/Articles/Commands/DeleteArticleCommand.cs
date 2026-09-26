using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Articles.Commands;

public record DeleteArticleCommand(int Id) : IRequest<Result>;
