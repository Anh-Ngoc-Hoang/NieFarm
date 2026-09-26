using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Articles.Dtos;
using NieFarm.Application.Features.Articles.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Articles.Commands;

public class SaveArticleCommandHandler(
    IRepositoryBase<Article> repository,
    IReadRepositoryBase<ArticleCategory> categories,
    IImageStorage imageStorage,
    ISlugGenerator slugGenerator)
    : IRequestHandler<SaveArticleCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveArticleCommand request, CancellationToken cancellationToken)
    {
        // Existence check only — the entity is discarded, so a read repository is safe here.
        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<int>.Error("Select an existing category.");

        var slug = slugGenerator.Generate(
            string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);

        var clash = await repository.FirstOrDefaultAsync(
            new ArticleBySlugSpec(slug, request.Id), cancellationToken);
        if (clash is not null)
            return Result<int>.Error($"Slug \"{slug}\" is already used by another article.");

        if (request.Id is { } id)
        {
            var existing = await repository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return Result<int>.NotFound();

            var previousImages = new[] { existing.ImageUrl, existing.HeroImageUrl };

            existing.Update(
                request.Title, slug, request.CategoryId, request.Author, request.Summary,
                request.ImageUrl, request.ImageAlt, request.HeroImageUrl, request.HeroImageAlt,
                request.Body, request.PublishedOn, request.IsPublished);

            await repository.UpdateAsync(existing, cancellationToken);

            var kept = new[] { existing.ImageUrl, existing.HeroImageUrl };
            var orphaned = previousImages
                .Where(url => ArticleImageLimits.IsManagedImage(url)
                              && !kept.Contains(url, StringComparer.OrdinalIgnoreCase));

            await imageStorage.DeleteManyAsync(orphaned, cancellationToken);

            return Result<int>.Success(existing.Id);
        }

        var article = Article.Create(
            request.Title, slug, request.CategoryId, request.Author, request.Summary,
            request.ImageUrl, request.ImageAlt, request.HeroImageUrl, request.HeroImageAlt,
            request.Body, request.PublishedOn, request.IsPublished);

        await repository.AddAsync(article, cancellationToken);
        return Result<int>.Success(article.Id);
    }
}
