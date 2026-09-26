using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.ArticleCategories.Commands;

public class SaveArticleCategoryCommandHandler(
    IRepositoryBase<ArticleCategory> repository,
    ISlugGenerator slugGenerator)
    : IRequestHandler<SaveArticleCategoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        SaveArticleCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? slugGenerator.Generate(request.Name)
            : slugGenerator.Generate(request.Slug);

        // Loaded through the write repository so the entity is tracked by the same context
        // that saves it — see .claude/rules/data-access.md.
        var all = await repository.ListAsync(cancellationToken);

        if (all.Any(c => c.Slug == slug && c.Id != request.Id))
            return Result<int>.Error($"Slug \"{slug}\" is already used by another category.");

        if (request.Id is { } id)
        {
            var existing = all.FirstOrDefault(c => c.Id == id);
            if (existing is null)
                return Result<int>.NotFound();

            existing.Update(request.Name, slug, request.IsVisible, request.SortOrder);
            await repository.UpdateAsync(existing, cancellationToken);
            return Result<int>.Success(existing.Id);
        }

        var category = ArticleCategory.Create(request.Name, slug, request.IsVisible, request.SortOrder);
        await repository.AddAsync(category, cancellationToken);
        return Result<int>.Success(category.Id);
    }
}
