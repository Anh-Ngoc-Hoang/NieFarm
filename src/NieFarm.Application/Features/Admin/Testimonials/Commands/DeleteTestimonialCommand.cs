using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Testimonials.Commands;

public record DeleteTestimonialCommand(int Id) : IRequest<Result>;
