using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.Testimonials.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Testimonials.Commands;

public class DeleteTestimonialCommandHandler(
    IRepositoryBase<Testimonial> repository,
    IImageStorage imageStorage)
    : IRequestHandler<DeleteTestimonialCommand, Result>
{
    public async Task<Result> Handle(DeleteTestimonialCommand request, CancellationToken cancellationToken)
    {
        var testimonial = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (testimonial is null)
            return Result.NotFound();

        var images = new[] { testimonial.PhotoUrl }
            .Where(TestimonialImageLimits.IsManagedImage)
            .ToList();

        await repository.DeleteAsync(testimonial, cancellationToken);
        await imageStorage.DeleteManyAsync(images, cancellationToken);

        return Result.Success();
    }
}
