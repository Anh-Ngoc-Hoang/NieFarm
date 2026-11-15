using MediatR;
using Microsoft.Extensions.Logging;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Application.Features.Orders.Notifications;

/// <summary>
/// Forwards a saved payment change to <see cref="IPaymentStatusPublisher"/> so the customer's open page
/// updates. The notification is published after the save, so a UI failure must never fail or retry a
/// payment: every exception is swallowed and logged.
/// </summary>
public class PublishPaymentStatusHandler(
    IPaymentStatusPublisher publisher,
    ILogger<PublishPaymentStatusHandler> logger) : INotificationHandler<OrderPaymentChangedNotification>
{
    public async Task Handle(OrderPaymentChangedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await publisher.PublishAsync(notification.Code, notification.To, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Publishing the payment status of order {Code} to open pages failed", notification.Code);
        }
    }
}
