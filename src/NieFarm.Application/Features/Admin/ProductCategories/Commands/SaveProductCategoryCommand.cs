using Ardalis.Result;
using MediatR;

namespace NieFarm.Application.Features.Admin.ProductCategories.Commands;

/// <summary>Creates when <paramref name="Id"/> is null, otherwise updates that row.</summary>
public record SaveProductCategoryCommand(
    int? Id,
    string Name,
    string? Slug,
    bool IsVisible,
    int SortOrder) : IRequest<Result<int>>;
