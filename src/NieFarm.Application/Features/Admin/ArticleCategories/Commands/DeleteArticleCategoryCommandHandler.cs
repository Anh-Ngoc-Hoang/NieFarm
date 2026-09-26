using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.ArticleCategories.Commands;

public class DeleteArticleCategoryCommandHandler(
    IRepositoryBase<ArticleCategory> repository,
    IReadRepositoryBase<Article> articles)
    : IRequestHandler<DeleteArticleCategoryCommand, Result>
{
    public async Task<Result> Handle(
        DeleteArticleCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.NotFound();

        var inUse = await articles.AnyAsync(cancellationToken);
        if (inUse)
        {
            var all = await articles.ListAsync(cancellationToken);
            var count = all.Count(p => p.CategoryId == request.Id);
            if (count > 0)
                return Result.Error($"This category still has {count} article(s). Move or delete them first.");
        }

        await repository.DeleteAsync(category, cancellationToken);
        return Result.Success();
    }
}
