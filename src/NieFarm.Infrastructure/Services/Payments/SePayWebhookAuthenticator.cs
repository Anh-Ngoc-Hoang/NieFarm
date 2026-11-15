using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NieFarm.Application.Common.Interfaces;

namespace NieFarm.Infrastructure.Services.Payments;

/// <summary>
/// Verifies SePay's HMAC-SHA256 signature: header "X-SePay-Signature: sha256={hex}" over
/// "{X-SePay-Timestamp}.{raw body}", compared in constant time, with the timestamp inside a window.
/// Enabled only when the section is on and the secret has at least 32 characters; otherwise one Error
/// is logged per options change and the endpoint answers 404. The secret never leaves this class.
/// </summary>
internal sealed class SePayWebhookAuthenticator(
    IOptionsMonitor<SePayOptions> monitor,
    ILogger<SePayWebhookAuthenticator> logger) : IPaymentWebhookAuthenticator
{
    private const string SignaturePrefix = "sha256=";

    private readonly object _gate = new();
    private SePayOptions? _checkedFor;
    private bool _enabled;
    private SePayOptions? _toleranceCheckedFor;
    private long _toleranceSeconds;
    private const long DefaultToleranceSeconds = 300;

    public bool IsEnabled
    {
        get
        {
            var options = monitor.CurrentValue;
            lock (_gate)
            {
                if (!ReferenceEquals(_checkedFor, options))
                {
                    _enabled = options.Enabled && (options.SecretKey?.Length ?? 0) >= SePayOptions.MinSecretKeyLength;
                    if (options.Enabled && !_enabled)
                        logger.LogError(
                            "Payments:SePay is enabled but SecretKey is shorter than {Min} characters; the webhook stays disabled.",
                            SePayOptions.MinSecretKeyLength);
                    _checkedFor = options;
                }

                return _enabled;
            }
        }
    }

    // verify: the signed string "{timestamp}.{raw body}", HMAC-SHA256 with the secret, "sha256=" + lower-case
    // hex, is taken from SePay's description (developer.sepay.vn/vi/sepay-webhooks/xac-thuc, read 2026-11-08);
    // SePay publishes only a PHP example. Prove it in SePay Test Mode, including whether a retry is re-signed
    // with a fresh timestamp.
    public bool Authenticate(string? signatureHeader, string? timestampHeader, byte[] rawBody)
    {
        if (!IsEnabled || string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(timestampHeader))
            return false;

        var options = monitor.CurrentValue;

        if (!long.TryParse(timestampHeader, NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp))
            return false;

        var tolerance = ToleranceFor(options);
        if (tolerance > 0)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Math.Abs(now - timestamp) > tolerance)
                return false;
        }

        var prefix = Encoding.UTF8.GetBytes(timestampHeader + ".");
        var message = new byte[prefix.Length + rawBody.Length];
        prefix.CopyTo(message, 0);
        rawBody.CopyTo(message, prefix.Length);

        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(options.SecretKey), message);
        var expected = Encoding.UTF8.GetBytes(SignaturePrefix + Convert.ToHexString(hash).ToLowerInvariant());
        var supplied = Encoding.UTF8.GetBytes(signatureHeader.Trim());

        if (supplied.Length != expected.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(supplied, expected);
    }

    // A negative tolerance is a misconfiguration (0 = window off): warn once per options instance, use 300.
    private long ToleranceFor(SePayOptions options)
    {
        lock (_gate)
        {
            if (!ReferenceEquals(_toleranceCheckedFor, options))
            {
                _toleranceSeconds = options.TimestampToleranceSeconds;
                if (_toleranceSeconds < 0)
                {
                    logger.LogWarning(
                        "Payments:SePay TimestampToleranceSeconds is negative ({Value}); using the default of {Default} seconds. Use 0 to turn the window off.",
                        options.TimestampToleranceSeconds, DefaultToleranceSeconds);
                    _toleranceSeconds = DefaultToleranceSeconds;
                }
                _toleranceCheckedFor = options;
            }

            return _toleranceSeconds;
        }
    }
}
