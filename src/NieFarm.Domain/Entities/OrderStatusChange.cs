using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One immutable row in an <see cref="Order"/>'s status history. Owned by the aggregate root —
/// only <see cref="Order"/> creates these.
/// </summary>
public class OrderStatusChange : BaseEntity
{
    public const int NoteMaxLength = 500;
    public const int NameMaxLength = 150;

    public int OrderId { get; private set; }

    /// <summary>Null means "placed" (the order did not exist before this row).</summary>
    public OrderStatus? FromStatus { get; private set; }

    public OrderStatus ToStatus { get; private set; }

    /// <summary>UTC.</summary>
    public DateTime ChangedAt { get; private set; }

    public OrderStatusActor ChangedBy { get; private set; }

    /// <summary>Identity user id, kept as a plain string with no FK so deleting an account never erases the audit.</summary>
    public string? ChangedByUserId { get; private set; }

    /// <summary>Display-name snapshot taken when the change was made.</summary>
    public string? ChangedByName { get; private set; }

    public string? Note { get; private set; }

    private OrderStatusChange() { }

    internal static OrderStatusChange Placed(DateTime at, string? customerId)
    {
        return new OrderStatusChange
        {
            FromStatus = null,
            ToStatus = OrderStatus.New,
            ChangedAt = at,
            ChangedBy = OrderStatusActor.Customer,
            ChangedByUserId = string.IsNullOrWhiteSpace(customerId) ? null : customerId,
            CreatedAt = at,
            UpdatedAt = at
        };
    }

    internal static OrderStatusChange Create(
        OrderStatus from, OrderStatus to, OrderStatusActor actor,
        string? userId, string? name, string? note)
    {
        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (trimmedNote is { Length: > NoteMaxLength })
            throw new ArgumentException($"A status note cannot exceed {NoteMaxLength} characters.", nameof(note));

        var trimmedName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        if (trimmedName is { Length: > NameMaxLength })
            trimmedName = trimmedName[..NameMaxLength];

        var now = DateTime.UtcNow;
        return new OrderStatusChange
        {
            FromStatus = from,
            ToStatus = to,
            ChangedAt = now,
            ChangedBy = actor,
            ChangedByUserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            ChangedByName = trimmedName,
            Note = trimmedNote,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
