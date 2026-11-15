namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Authenticates payment-gateway webhook calls. The secret never leaves Infrastructure.
/// </summary>
public interface IPaymentWebhookAuthenticator
{
    /// <summary>True only when the webhook is switched on and its secret is configured strongly enough.</summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Verifies the HMAC-SHA256 signature over the timestamp and the raw body, in constant time, and
    /// that the timestamp is inside the allowed window. Any missing or wrong input returns false.
    /// </summary>
    bool Authenticate(string? signatureHeader, string? timestampHeader, byte[] rawBody);
}
