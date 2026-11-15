using NieFarm.Application.Features.Payments.Dtos;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Dtos;

public record MyOrderLineDto(
    string ProductName, string Variant, string ImageUrl,
    decimal UnitPrice, int Quantity, decimal LineTotal);

public record MyOrderDetailDto(
    int Id, string Code, string CustomerName, string Phone, string? Email,
    string ShippingAddress, string? Note, OrderStatus Status, PaymentMethod PaymentMethod,
    decimal Subtotal, decimal ShippingFee, decimal Discount, decimal Total,
    DateTime PlacedAt, List<MyOrderLineDto> Lines,
    bool CanCancel, IReadOnlyList<OrderTimelineEntryDto> Timeline,
    PaymentStatus PaymentStatus, DateTime? PaidAtUtc, BankTransferInstructionsDto? BankTransfer,
    string? ShippingZoneName = null);
