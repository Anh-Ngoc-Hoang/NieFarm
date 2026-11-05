using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Testimonials.Commands;

/// <summary>Creates when <paramref name="Id"/> is null, otherwise updates that row.</summary>
public record SaveTestimonialCommand(
    int? Id,
    string Quote,
    string AuthorName,
    string? AuthorRole,
    string? PhotoUrl,
    int Rating,
    int SortOrder,
    bool IsVisible) : IRequest<Result<int>>;
