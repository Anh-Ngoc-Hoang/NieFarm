using Ardalis.Result;
using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Payments.Commands;

/// <summary>
/// A money-in notification from a payment gateway (today SePay). Provider-agnostic: a later PayOS or
/// Casso endpoint reuses it. TransferType is the gateway value ("in" for money received). Every
/// business outcome is a successful Result carrying a <see cref="GatewayPaymentOutcome"/>; only a
/// concurrency conflict is a failure (so the gateway retries).
/// </summary>
public record ConfirmPaymentFromGatewayCommand(
    string Provider,
    string ExternalId,
    string TransferType,
    string? AccountNumber,
    decimal Amount,
    string? Content,
    string? Code,
    string? Reference) : IRequest<Result<GatewayPaymentOutcome>>;
