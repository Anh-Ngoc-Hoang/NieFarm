using MediatR;
using NieFarm.Application.Features.Admin.Testimonials.Dtos;

namespace NieFarm.Application.Features.Admin.Testimonials.Queries;

public record GetTestimonialsAdminQuery : IRequest<List<TestimonialAdminListDto>>;
