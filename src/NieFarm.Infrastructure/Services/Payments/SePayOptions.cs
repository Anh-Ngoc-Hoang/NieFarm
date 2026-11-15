namespace NieFarm.Infrastructure.Services.Payments;

/// <summary>Section Payments:SePay. SecretKey is a secret: never commit it (appsettings.Development.json locally, environment variable in production).</summary>
public class SePayOptions
{
    public const string SectionName = "Payments:SePay";

    /// <summary>Minimum secret length for the webhook to be considered enabled.</summary>
    public const int MinSecretKeyLength = 32;

    public bool Enabled { get; set; }
    public string SecretKey { get; set; } = "";

    /// <summary>Allowed distance between the signed timestamp and now, in seconds. 0 switches the window off.</summary>
    public int TimestampToleranceSeconds { get; set; } = 300;
}
