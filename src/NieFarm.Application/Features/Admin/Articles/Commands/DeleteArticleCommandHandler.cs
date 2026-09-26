using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Articles.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Articles.Commands;

public class DeleteArticleCommandHandler(
    IRepositoryBase<Article> repository,
    IImageStorage imageStorage)
    : IRequestHandler<DeleteArticleCommand, Result>
{
    public async Task<Result> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (article is null)
            return Result.NotFound();

        var images = new[] { article.ImageUrl, article.HeroImageUrl }
            .Where(ArticleImageLimits.IsManagedImage)
            .ToList();

        await repository.DeleteAsync(article, cancellationToken);
        await imageStorage.DeleteManyAsync(images, cancellationToken);

        return Result.Success();
    }
}
