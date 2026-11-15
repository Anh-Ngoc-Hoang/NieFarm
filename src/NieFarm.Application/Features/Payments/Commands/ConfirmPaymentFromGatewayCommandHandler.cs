using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Orders;
using NieFarm.Application.Features.Orders.Specifications;
using NieFarm.Application.Features.Payments.Specifications;
using NieFarm.Domain.Entities;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Payments.Commands;

/// <summary>
/// Matches an incoming bank transfer to an order by the order code in the transfer content and
/// records the payment. Never changes Order.Status. Logs only provider, external id, outcome and
/// the order code: never the content, account number or amount.
/// <para>
/// Every accepted delivery leaves one GatewayTransaction row (the audit and inbox record). The order
/// and its payment history stay the source of truth for money. Ignored outcomes add the row and let a
/// failed save surface (the endpoint answers 500 so the gateway retries). Recorded confirms the order
/// first and then adds the row; if only that second save fails the money is already recorded, so it
/// logs an Error and still succeeds.
/// </para>
/// </summary>
public class ConfirmPaymentFromGatewayCommandHandler(
    IReadRepositoryBase<Order> readRepository,
    IRepositoryBase<Order> repository,
    IReadRepositoryBase<GatewayTransaction> readTransactions,
    IRepositoryBase<GatewayTransaction> transactions,
    IWriteContextRefresher refresher,
    IBankTransferAccountProvider accountProvider,
    OrderPaymentWorkflow workflow,
    ILogger<ConfirmPaymentFromGatewayCommandHandler> logger)
    : IRequestHandler<ConfirmPaymentFromGatewayCommand, Result<GatewayPaymentOutcome>>
{
    public async Task<Result<GatewayPaymentOutcome>> Handle(
        ConfirmPaymentFromGatewayCommand request, CancellationToken cancellationToken)
    {
        // Duplicate delivery comes first so a re-sent id is never classified or written again. Both are
        // value checks (nothing loaded here is passed to a write repository). The unique indexes on
        // (Provider, ExternalId) are the backstop: if two deliveries race, the loser fails its save, the
        // gateway retries, and this check then wins.
        if (await readTransactions.AnyAsync(
                new GatewayTransactionByExternalIdSpec(request.Provider, request.ExternalId), cancellationToken)
            || await readRepository.AnyAsync(
                new OrderByPaymentExternalIdSpec(request.Provider, request.ExternalId), cancellationToken))
            return Done(request, GatewayPaymentOutcome.Duplicate, null, LogLevel.Information);

        if (!string.Equals(request.TransferType, "in", StringComparison.OrdinalIgnoreCase))
            return await IgnoreAsync(request, GatewayPaymentOutcome.NotIncoming, null, null, LogLevel.Debug, cancellationToken);

        // A second account linked to the same gateway login must never confirm an order.
        var account = accountProvider.GetAccount();
        if (account is null || !string.Equals(
                request.AccountNumber?.Trim(), account.AccountNumber, StringComparison.Ordinal))
            return await IgnoreAsync(request, GatewayPaymentOutcome.WrongAccount, null, null, LogLevel.Warning, cancellationToken);

        if (!OrderCode.TryFind(request.Content, request.Code, out var code, out var ambiguous))
            return await IgnoreAsync(
                request,
                ambiguous ? GatewayPaymentOutcome.AmbiguousOrder : GatewayPaymentOutcome.NoOrderFound,
                null, null, LogLevel.Warning, cancellationToken);

        // Decide against the database, then load for write with Items (Total is needed).
        refresher.DiscardTrackedEntities();
        var order = await repository.FirstOrDefaultAsync(new OrderForPaymentByCodeSpec(code), cancellationToken);
        if (order is null)
            return await IgnoreAsync(request, GatewayPaymentOutcome.NoOrderFound, code, null, LogLevel.Warning, cancellationToken);

        // Already paid, refund due, refunded, COD: nothing is recorded or refunded automatically.
        if (order.PaymentMethod != PaymentMethod.BankTransfer || !order.CanConfirmPayment)
            return await IgnoreAsync(request, GatewayPaymentOutcome.NotAwaitingPayment, code, order.Id, LogLevel.Warning, cancellationToken);

        // No partial-payment state: an admin handles an underpayment by hand.
        if (request.Amount < order.Total)
            return await IgnoreAsync(request, GatewayPaymentOutcome.Underpaid, code, order.Id, LogLevel.Warning, cancellationToken);

        var reference = Truncate(request.Reference, OrderPaymentChange.ReferenceMaxLength);
        var note = request.Amount > order.Total
            ? $"Received {request.Amount:0}, order total {order.Total:0}"
            : null;

        var orderId = order.Id;
        var result = await workflow.ConfirmAsync(
            order, OrderStatusActor.System, null, request.Provider, reference, note,
            request.Amount, request.Provider, request.ExternalId, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Conflict)
            {
                logger.LogWarning(
                    "Gateway payment {Provider} {ExternalId} hit a concurrency conflict for order {Code}; the gateway should retry",
                    request.Provider, request.ExternalId, code);
                return Result<GatewayPaymentOutcome>.Conflict();
            }

            logger.LogWarning(
                "Gateway payment {Provider} {ExternalId} was not recorded for order {Code}: {Status}",
                request.Provider, request.ExternalId, code, result.Status);
            return Result<GatewayPaymentOutcome>.Error("The payment could not be recorded.");
        }

        // The money is already saved, so the request token no longer applies (an aborted request must not
        // lose the audit row). If the row fails for any reason, a retry could only report Duplicate (the
        // payment history carries the same key), so log an Error and still succeed.
        try
        {
            await transactions.AddAsync(
                NewRow(request, GatewayPaymentOutcome.Recorded, code, orderId), CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Gateway payment {Provider} {ExternalId} was recorded on order {Code} but its GatewayTransaction row could not be saved",
                request.Provider, request.ExternalId, code);
        }

        return Done(request, GatewayPaymentOutcome.Recorded, code, LogLevel.Information);
    }

    // An ignored delivery must not be lost: persist the audit row, and let a failed save propagate so
    // the endpoint answers 500 and the gateway retries.
    private async Task<Result<GatewayPaymentOutcome>> IgnoreAsync(
        ConfirmPaymentFromGatewayCommand request, GatewayPaymentOutcome outcome, string? code, int? orderId,
        LogLevel level, CancellationToken ct)
    {
        await transactions.AddAsync(NewRow(request, outcome, code, orderId), ct);
        return Done(request, outcome, code, level);
    }

    private static GatewayTransaction NewRow(
        ConfirmPaymentFromGatewayCommand request, GatewayPaymentOutcome outcome, string? code, int? orderId) =>
        GatewayTransaction.Create(
            request.Provider, request.ExternalId, request.TransferType, request.AccountNumber,
            request.Amount, request.Content, code ?? request.Code, request.Reference, outcome, orderId);

    private Result<GatewayPaymentOutcome> Done(
        ConfirmPaymentFromGatewayCommand request, GatewayPaymentOutcome outcome, string? code, LogLevel level)
    {
        logger.Log(level,
            "Gateway payment {Provider} {ExternalId} outcome {Outcome} order {Code}",
            request.Provider, request.ExternalId, outcome, code ?? "-");
        return Result<GatewayPaymentOutcome>.Success(outcome);
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
