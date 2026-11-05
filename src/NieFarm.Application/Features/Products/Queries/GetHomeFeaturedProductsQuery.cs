using MediatR;
using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Products.Queries;

/// <summary>
/// A bare list, not Result&lt;…&gt;, because it has no failure mode (mirrors
/// GetShopProductsQuery/GetActivitiesQuery). <paramref name="Take"/> is clamped to 1..8 by the
/// handler.
/// </summary>
public record GetHomeFeaturedProductsQuery(int Take = 4) : IRequest<IReadOnlyList<ProductCardDto>>;
