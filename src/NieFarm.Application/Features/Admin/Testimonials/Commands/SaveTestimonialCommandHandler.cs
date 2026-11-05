using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Testimonials.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Testimonials.Commands;

public class SaveTestimonialCommandHandler(
    IRepositoryBase<Testimonial> repository,
    IImageStorage imageStorage)
    : IRequestHandler<SaveTestimonialCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveTestimonialCommand request, CancellationToken cancellationToken)
    {
        if (request.Id is { } id)
        {
            var existing = await repository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return Result<int>.NotFound();

            var previousPhotoUrl = existing.PhotoUrl;

            existing.Update(
                request.Quote,
                request.AuthorName,
                request.AuthorRole ?? string.Empty,
                request.PhotoUrl ?? string.Empty,
                request.Rating,
                request.SortOrder,
                request.IsVisible);

            await repository.UpdateAsync(existing, cancellationToken);

            if (TestimonialImageLimits.IsManagedImage(previousPhotoUrl)
                && !string.Equals(previousPhotoUrl, existing.PhotoUrl, StringComparison.OrdinalIgnoreCase))
            {
                await imageStorage.DeleteManyAsync([previousPhotoUrl], cancellationToken);
            }

            return Result<int>.Success(existing.Id);
        }

        var testimonial = Testimonial.Create(
            request.Quote,
            request.AuthorName,
            request.AuthorRole ?? string.Empty,
            request.PhotoUrl ?? string.Empty,
            request.Rating,
            request.SortOrder,
            request.IsVisible);

        await repository.AddAsync(testimonial, cancellationToken);
        return Result<int>.Success(testimonial.Id);
    }
}
