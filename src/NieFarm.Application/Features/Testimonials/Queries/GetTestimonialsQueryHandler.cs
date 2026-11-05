using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Testimonials.Dtos;
using NieFarm.Application.Features.Testimonials.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Testimonials.Queries;

public class GetTestimonialsQueryHandler(IReadRepositoryBase<Testimonial> repository)
    : IRequestHandler<GetTestimonialsQuery, List<TestimonialDto>>
{
    public async Task<List<TestimonialDto>> Handle(
        GetTestimonialsQuery request, CancellationToken cancellationToken)
    {
        var testimonials = await repository.ListAsync(new VisibleTestimonialsSpec(), cancellationToken);

        return testimonials.Select(t => new TestimonialDto(
            t.Id,
            t.Quote,
            t.AuthorName,
            t.AuthorRole,
            string.IsNullOrWhiteSpace(t.PhotoUrl) ? null : t.PhotoUrl,
            t.Rating)).ToList();
    }
}
