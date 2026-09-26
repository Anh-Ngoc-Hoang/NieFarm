using MediatR;
using NieFarm.Application.Features.Admin.ProductCategories.Dtos;

namespace NieFarm.Application.Features.Admin.ProductCategories.Queries;

public record GetProductCategoriesQuery : IRequest<List<ProductCategoryDto>>;
