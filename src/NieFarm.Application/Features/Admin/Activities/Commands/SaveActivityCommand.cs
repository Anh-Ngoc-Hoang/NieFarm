using Ardalis.Result;
using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Activities.Commands;

/// <summary>
/// Creates when <paramref name="Id"/> is null, otherwise updates that row.
/// A blank <paramref name="Slug"/> is generated from the title.
/// </summary>
public record SaveActivityCommand(
    int? Id,
    string Title,
    string? Slug,
    string? ModalTitle,
    DateOnly EventDate,
    string ImageUrl,
    string ImageAlt,
    ActivityCardSize CardSize,
    string Body,
    bool IsPublished) : IRequest<Result<int>>;
