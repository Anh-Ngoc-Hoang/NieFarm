using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Dtos;

public record OrderAdminListDto(
    int Id,
    string Code,
    string CustomerName,
    string Phone,
    OrderStatus Status,
    bool IsRead,
    int ItemCount,
    decimal Total,
    DateTime PlacedAt);
