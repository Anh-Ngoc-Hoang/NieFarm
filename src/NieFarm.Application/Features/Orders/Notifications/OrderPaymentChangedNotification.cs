using MediatR;
using NieFarm.Domain.Enums;

namespace NieFarm.Application.Features.Orders.Notifications;

/// <summary>
/// Published after a payment state change is saved (confirm, revert, refund, or a cancel that flipped
/// Paid to RefundDue). Handled today by the live payment status handler (D18); R-03 adds the email
/// ones.
/// </summary>
public record OrderPaymentChangedNotification(
    int OrderId, string Code, PaymentStatus? From, PaymentStatus To,
    OrderStatusActor Actor, DateTime ChangedAtUtc) : INotification;
