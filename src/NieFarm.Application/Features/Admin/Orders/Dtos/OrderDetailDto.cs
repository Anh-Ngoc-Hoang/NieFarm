using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Admin.Orders.Dtos;

public record OrderLineDto(
    string ProductName,
    string Variant,
    string ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderStatusChangeDto(
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    DateTime ChangedAtUtc,
    OrderStatusActor ChangedBy,
    bool ByAccount,
    string? ChangedByName,
    string? Note);

public record OrderDetailDto(
    int Id,
    string Code,
    string CustomerName,
    string Phone,
    string? Email,
    string ShippingAddress,
    string? Note,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Discount,
    decimal Total,
    DateTime PlacedAt,
    List<OrderLineDto> Lines,
    IReadOnlyList<OrderStatus> AllowedNextStatuses,
    List<OrderStatusChangeDto> History);
