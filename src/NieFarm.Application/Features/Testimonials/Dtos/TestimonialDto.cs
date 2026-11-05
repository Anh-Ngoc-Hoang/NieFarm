namespace NieFarm.Application.Features.Testimonials.Dtos;

/// <summary>A public-facing testimonial. <see cref="PhotoUrl"/> is null when the row has no photo.</summary>
public record TestimonialDto(
    int Id,
    string Quote,
    string AuthorName,
    string AuthorRole,
    string? PhotoUrl,
    int Rating);
