using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.Testimonials.Dtos;
using NieFarm.Application.Features.Testimonials.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.Testimonials.Queries;

public class GetTestimonialsAdminQueryHandler(IReadRepositoryBase<Testimonial> repository)
    : IRequestHandler<GetTestimonialsAdminQuery, List<TestimonialAdminListDto>>
{
    public async Task<List<TestimonialAdminListDto>> Handle(
        GetTestimonialsAdminQuery request, CancellationToken cancellationToken)
    {
        var testimonials = await repository.ListAsync(new AllTestimonialsAdminSpec(), cancellationToken);

        return testimonials.Select(t => new TestimonialAdminListDto(
            t.Id,
            t.Quote,
            t.AuthorName,
            t.AuthorRole,
            t.PhotoUrl,
            t.Rating,
            t.SortOrder,
            t.IsVisible)).ToList();
    }
}
