namespace NieFarm.Infrastructure.Services.Payments;

/// <summary>Section Payments:BankTransfer. Real values live in appsettings.Development.json or environment variables.</summary>
public class BankTransferOptions
{
    public const string SectionName = "Payments:BankTransfer";

    public bool Enabled { get; set; }

    /// <summary>6-digit NAPAS acquirer BIN, e.g. 970436.</summary>
    public string BankBin { get; set; } = "";

    public string BankName { get; set; } = "";
    public string AccountNumber { get; set; } = "";

    /// <summary>Upper-case, as registered with the bank.</summary>
    public string AccountName { get; set; } = "";

    public string? Branch { get; set; }
}
