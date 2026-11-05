using MediatR;
using NieFarm.Application.Features.Testimonials.Dtos;

namespace NieFarm.Application.Features.Testimonials.Queries;

/// <summary>
/// A bare list, not Result&lt;List&lt;TestimonialDto&gt;&gt;, because it has no failure mode
/// (mirrors GetActivitiesQuery). Public content — no scoping, no ICurrentUser.
/// </summary>
public record GetTestimonialsQuery : IRequest<List<TestimonialDto>>;
