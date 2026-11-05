using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Testimonials.Dtos;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Testimonials.Queries;

public class GetTestimonialForEditQueryHandler(IReadRepositoryBase<Testimonial> repository)
    : IRequestHandler<GetTestimonialForEditQuery, TestimonialEditDto?>
{
    public async Task<TestimonialEditDto?> Handle(
        GetTestimonialForEditQuery request, CancellationToken cancellationToken)
    {
        var testimonial = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (testimonial is null)
            return null;

        return new TestimonialEditDto(
            testimonial.Id,
            testimonial.Quote,
            testimonial.AuthorName,
            testimonial.AuthorRole,
            testimonial.PhotoUrl,
            testimonial.Rating,
            testimonial.SortOrder,
            testimonial.IsVisible);
    }
}
