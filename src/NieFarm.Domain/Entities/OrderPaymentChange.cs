using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// One immutable row in an <see cref="Order"/>'s payment history. Owned by the aggregate root -
/// only <see cref="Order"/> creates these. There is no "placed" row: the initial state is implied by
/// the payment method, and history holds only events.
/// </summary>
public class OrderPaymentChange : BaseEntity
{
    public const int NoteMaxLength = 500;
    public const int NameMaxLength = 150;
    public const int ReferenceMaxLength = 100;
    public const int ProviderMaxLength = 30;
    public const int ExternalIdMaxLength = 64;

    public int OrderId { get; private set; }

    public PaymentStatus? FromStatus { get; private set; }

    public PaymentStatus ToStatus { get; private set; }

    /// <summary>UTC.</summary>
    public DateTime ChangedAt { get; private set; }

    public OrderStatusActor ChangedBy { get; private set; }

    /// <summary>Identity user id, plain string with no FK so deleting an account never erases the audit.</summary>
    public string? ChangedByUserId { get; private set; }

    /// <summary>Display-name snapshot taken when the change was made.</summary>
    public string? ChangedByName { get; private set; }

    public decimal? Amount { get; private set; }

    /// <summary>Bank transaction reference (typed by the admin, or the gateway's reference code).</summary>
    public string? Reference { get; private set; }

    /// <summary>Gateway name for automatic rows (e.g. "SePay"); null for manual ones.</summary>
    public string? Provider { get; private set; }

    /// <summary>The gateway's transaction id; with <see cref="Provider"/> it is the idempotency key.</summary>
    public string? ExternalId { get; private set; }

    public string? Note { get; private set; }

    private OrderPaymentChange() { }

    internal static OrderPaymentChange Create(
        PaymentStatus? from, PaymentStatus to, OrderStatusActor actor,
        string? userId, string? name, decimal? amount,
        string? reference, string? note, string? provider = null, string? externalId = null)
    {
        var trimmedNote = Clean(note);
        if (trimmedNote is { Length: > NoteMaxLength })
            throw new ArgumentException($"A payment note cannot exceed {NoteMaxLength} characters.", nameof(note));

        var trimmedReference = Clean(reference);
        if (trimmedReference is { Length: > ReferenceMaxLength })
            throw new ArgumentException($"A payment reference cannot exceed {ReferenceMaxLength} characters.", nameof(reference));

        var trimmedProvider = Clean(provider);
        if (trimmedProvider is { Length: > ProviderMaxLength })
            throw new ArgumentException($"A payment provider cannot exceed {ProviderMaxLength} characters.", nameof(provider));

        var trimmedExternalId = Clean(externalId);
        if (trimmedExternalId is { Length: > ExternalIdMaxLength })
            throw new ArgumentException($"An external id cannot exceed {ExternalIdMaxLength} characters.", nameof(externalId));

        var trimmedName = Clean(name);
        if (trimmedName is { Length: > NameMaxLength })
            trimmedName = trimmedName[..NameMaxLength];

        var now = DateTime.UtcNow;
        return new OrderPaymentChange
        {
            FromStatus = from,
            ToStatus = to,
            ChangedAt = now,
            ChangedBy = actor,
            ChangedByUserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            ChangedByName = trimmedName,
            Amount = amount,
            Reference = trimmedReference,
            Provider = trimmedProvider,
            ExternalId = trimmedExternalId,
            Note = trimmedNote,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
