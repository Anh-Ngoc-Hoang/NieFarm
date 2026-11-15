namespace NieFarm.Domain.Enums;

/// <summary>
/// Business outcome of a gateway webhook delivery. Stored as an int on GatewayTransaction: never
/// renumber. Every outcome is acknowledged to the gateway.
/// </summary>
public enum GatewayPaymentOutcome
{
    Recorded = 0,
    Duplicate = 1,
    NotIncoming = 2,
    WrongAccount = 3,
    NoOrderFound = 4,
    AmbiguousOrder = 5,
    NotAwaitingPayment = 6,
    Underpaid = 7
}
