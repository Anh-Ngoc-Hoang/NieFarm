namespace NieFarm.Application.Features.Admin.Testimonials.Dtos;

public record TestimonialAdminListDto(
    int Id,
    string Quote,
    string AuthorName,
    string AuthorRole,
    string PhotoUrl,
    int Rating,
    int SortOrder,
    bool IsVisible);
