using NieFarm.Application.Features.Payments.Dtos;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Dtos;

public record OrderLookupLineDto(
    string ProductName, string Variant, string ImageUrl, decimal UnitPrice, int Quantity, decimal LineTotal);

/// <summary>Minimal-PII view for the code + phone lookup. RecipientName and PhoneMasked are already masked.</summary>
public record OrderLookupDto(
    string Code,
    DateTime PlacedAtUtc,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    string RecipientName,
    string PhoneMasked,
    string DeliveryArea,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Discount,
    decimal Total,
    IReadOnlyList<OrderLookupLineDto> Lines,
    bool CanCancel,
    bool CancelRequiresSignIn,
    IReadOnlyList<OrderTimelineEntryDto> Timeline,
    PaymentStatus PaymentStatus,
    DateTime? PaidAtUtc,
    BankTransferInstructionsDto? BankTransfer,
    string? ShippingZoneName = null,
    string? CouponCode = null);
