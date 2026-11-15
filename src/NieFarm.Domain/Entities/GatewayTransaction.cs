using NieFarm.Domain.Common;
using NieFarm.Domain.Enums;

namespace NieFarm.Domain.Entities;

/// <summary>
/// Durable audit and inbox record of every payment-gateway webhook the site accepted. Deliberately NOT
/// a child of <see cref="Order"/> and with NO foreign key, so the audit survives order deletion. The
/// order and its payment history remain the source of truth for money; this row is the record that
/// something arrived and what was decided. Never stores the raw payload, the signature or the secret.
/// <see cref="BaseEntity.CreatedAt"/> is the receipt time.
/// </summary>
public class GatewayTransaction : BaseEntity
{
    public const int ProviderMaxLength = 30;
    public const int ExternalIdMaxLength = 64;
    public const int TransferTypeMaxLength = 10;
    public const int AccountNumberMaxLength = 50;
    public const int ContentMaxLength = 500;
    public const int CodeMaxLength = 50;
    public const int ReferenceMaxLength = 100;

    public string Provider { get; private set; } = "";
    public string ExternalId { get; private set; } = "";
    public string TransferType { get; private set; } = "";
    public string AccountNumber { get; private set; } = "";
    public decimal Amount { get; private set; }

    /// <summary>The transfer content, truncated to 500. May contain the payer's name.</summary>
    public string Content { get; private set; } = "";

    public string? Code { get; private set; }
    public string? Reference { get; private set; }
    public GatewayPaymentOutcome Outcome { get; private set; }

    /// <summary>Set when an order was matched. A plain int, no FK.</summary>
    public int? OrderId { get; private set; }

    private GatewayTransaction() { }

    public static GatewayTransaction Create(
        string provider, string externalId, string? transferType, string? accountNumber, decimal amount,
        string? content, string? code, string? reference, GatewayPaymentOutcome outcome, int? orderId)
    {
        if (string.IsNullOrWhiteSpace(provider) || provider.Trim().Length > ProviderMaxLength)
            throw new ArgumentException("Provider is required and at most 30 characters.", nameof(provider));
        if (string.IsNullOrWhiteSpace(externalId) || externalId.Trim().Length > ExternalIdMaxLength)
            throw new ArgumentException("External id is required and at most 64 characters.", nameof(externalId));

        return new GatewayTransaction
        {
            Provider = provider.Trim(),
            ExternalId = externalId.Trim(),
            TransferType = Truncate(transferType, TransferTypeMaxLength) ?? "",
            AccountNumber = Truncate(accountNumber, AccountNumberMaxLength) ?? "",
            Amount = amount,
            Content = Truncate(content, ContentMaxLength) ?? "",
            Code = Truncate(code, CodeMaxLength),
            Reference = Truncate(reference, ReferenceMaxLength),
            Outcome = outcome,
            OrderId = orderId
        };
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
