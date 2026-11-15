using NieFarm.Domain.Enums;

namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Pushes a payment status change to whoever is looking at that order right now (Web implements it
/// in memory). The message is only the order code and the new status: no amount, no names.
/// </summary>
public interface IPaymentStatusPublisher
{
    Task PublishAsync(string orderCode, PaymentStatus status, CancellationToken ct);
}
