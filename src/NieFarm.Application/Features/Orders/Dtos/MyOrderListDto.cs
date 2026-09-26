using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Dtos;

public record MyOrderListDto(
    int Id, string Code, OrderStatus Status, int ItemCount, decimal Total, DateTime PlacedAt);
