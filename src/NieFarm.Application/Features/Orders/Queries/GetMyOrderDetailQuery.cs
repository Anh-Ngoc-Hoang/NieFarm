using Ardalis.Result;
using MediatR;
using NieFarm.Application.Features.Orders.Dtos;

namespace NieFarm.Application.Features.Orders.Queries;

public record GetMyOrderDetailQuery(int Id) : IRequest<Result<MyOrderDetailDto>>;
