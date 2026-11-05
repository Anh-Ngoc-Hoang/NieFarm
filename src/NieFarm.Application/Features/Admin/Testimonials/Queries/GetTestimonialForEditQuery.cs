using MediatR;
using NieFarm.Application.Features.Admin.Testimonials.Dtos;

namespace NieFarm.Application.Features.Admin.Testimonials.Queries;

public record GetTestimonialForEditQuery(int Id) : IRequest<TestimonialEditDto?>;
