using NieFarm.Application.Features.Payments.Dtos;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Dtos;

/// <summary>
/// Result of placing an order. BankTransfer carries the payment instructions for a bank-transfer
/// order (null for COD or when the account became unavailable).
/// </summary>
public record OrderPlacedDto(
    int Id, string Code, PaymentMethod PaymentMethod, decimal Total, BankTransferInstructionsDto? BankTransfer);
