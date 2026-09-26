using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Activities.Specifications;
using NieFarm.Application.Features.Admin.Activities.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Activities.Commands;

public class SaveActivityCommandHandler(
    IRepositoryBase<Activity> repository,
    IImageStorage imageStorage,
    ISlugGenerator slugGenerator,
    IHtmlSanitizer htmlSanitizer)
    : IRequestHandler<SaveActivityCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveActivityCommand request, CancellationToken cancellationToken)
    {
        var slug = slugGenerator.Generate(
            string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);
        var body = htmlSanitizer.Sanitize(request.Body);

        var clash = await repository.FirstOrDefaultAsync(
            new ActivityBySlugSpec(slug, request.Id), cancellationToken);
        if (clash is not null)
            return Result<int>.Error($"Slug \"{slug}\" is already used by another activity.");

        if (request.Id is { } id)
        {
            var existing = await repository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return Result<int>.NotFound();

            var previousImageUrl = existing.ImageUrl;

            existing.Update(
                request.Title, slug, request.ModalTitle ?? string.Empty, request.EventDate,
                request.ImageUrl, request.ImageAlt, request.CardSize, body, request.IsPublished);

            await repository.UpdateAsync(existing, cancellationToken);

            if (ActivityImageLimits.IsManagedImage(previousImageUrl)
                && !string.Equals(previousImageUrl, existing.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                await imageStorage.DeleteManyAsync([previousImageUrl], cancellationToken);
            }

            return Result<int>.Success(existing.Id);
        }

        var activity = Activity.Create(
            request.Title, slug, request.ModalTitle ?? string.Empty, request.EventDate,
            request.ImageUrl, request.ImageAlt, request.CardSize, body, request.IsPublished);

        await repository.AddAsync(activity, cancellationToken);
        return Result<int>.Success(activity.Id);
    }
}
