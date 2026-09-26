using Ardalis.Result;
using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Commands;

public record UpdateOrderStatusCommand(int Id, OrderStatus Status) : IRequest<Result>;
