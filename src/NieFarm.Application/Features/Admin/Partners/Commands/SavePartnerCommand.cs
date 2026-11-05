using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.Partners.Commands;

/// <summary>Creates when <paramref name="Id"/> is null, otherwise updates that row.</summary>
public record SavePartnerCommand(
    int? Id,
    string Name,
    string? LogoUrl,
    string? WebsiteUrl,
    int SortOrder,
    bool IsVisible) : IRequest<Result<int>>;
