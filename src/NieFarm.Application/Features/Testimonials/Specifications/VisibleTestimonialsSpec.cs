using Ardalis.Specification;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Testimonials.Specifications;

/// <summary>Visible testimonials only, in display order — what the home page carousel reads.</summary>
public sealed class VisibleTestimonialsSpec : Specification<Testimonial>
{
    public VisibleTestimonialsSpec()
    {
        Query.Where(t => t.IsVisible)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Id)
            .Take(12);
    }
}
