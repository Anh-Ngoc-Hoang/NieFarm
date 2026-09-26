using MediatR;
using NieFarm.Application.Features.Admin.Orders.Dtos;

namespace NieFarm.Application.Features.Admin.Orders.Queries;

public record GetOrderDetailQuery(int Id) : IRequest<OrderDetailDto?>;
