using NieFarm.Application.Features.Payments.Dtos;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Payments;

/// <summary>
/// Builds the payment instructions for an order, or null when none should be shown: only a
/// bank-transfer order that is still awaiting payment, not cancelled, with a usable account.
/// Callers must have loaded Order.Items (the amount is Order.Total).
/// </summary>
public static class BankTransferInstructions
{
    public static BankTransferInstructionsDto? Build(Order order, BankTransferAccount? account)
    {
        if (account is null
            || order.PaymentMethod != PaymentMethod.BankTransfer
            || order.PaymentStatus != PaymentStatus.AwaitingPayment
            || order.Status == OrderStatus.Cancelled
            || order.Total <= 0)
            return null;

        var amount = decimal.Ceiling(order.Total);
        var payload = VietQrPayload.Build(account.BankBin, account.AccountNumber, amount, order.Code);

        return new BankTransferInstructionsDto(
            account.BankName, account.AccountNumber, account.AccountName, account.Branch,
            amount, order.Code, payload);
    }
}
