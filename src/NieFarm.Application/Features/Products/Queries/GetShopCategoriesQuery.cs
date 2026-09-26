using MediatR;
using NieFarm.Application.Features.Products.Dtos;

namespace NieFarm.Application.Features.Products.Queries;

/// <summary>The "Danh Mục" filter rail on /cua-hang, driven by visible ProductCategory rows.</summary>
public record GetShopCategoriesQuery : IRequest<IReadOnlyList<ProductCategoryFilterDto>>;
