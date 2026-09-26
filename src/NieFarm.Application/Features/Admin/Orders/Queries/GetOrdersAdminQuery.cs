using MediatR;
using NieFarm.Application.Features.Admin.Orders.Dtos;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

public record GetOrdersAdminQuery : IRequest<List<OrderAdminListDto>>;
