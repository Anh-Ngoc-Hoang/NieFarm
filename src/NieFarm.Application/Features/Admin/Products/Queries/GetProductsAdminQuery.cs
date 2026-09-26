using MediatR;
using NieFarm.Application.Features.Admin.Products.Dtos;

namespace NieFarm.Application.Features.Admin.Products.Queries;

public record GetProductsAdminQuery : IRequest<List<ProductAdminListDto>>;
