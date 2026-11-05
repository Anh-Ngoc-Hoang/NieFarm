using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Testimonials.Specifications;

/// <summary>Every testimonial, visible or not, in display order.</summary>
public sealed class AllTestimonialsAdminSpec : Specification<Testimonial>
{
    public AllTestimonialsAdminSpec()
    {
        Query.OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Id);
    }
}
