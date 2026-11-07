namespace NieFarm.Domain.Enums;

/// <summary>Who caused an order status change. Stored as an int — never renumber.</summary>
public enum OrderStatusActor
{
    /// <summary>Reserved for automated changes (payment webhooks, jobs). Unused today.</summary>
    System = 0,
    Customer = 1,
    Admin = 2
}
