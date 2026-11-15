using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Payments;

namespace NieFarm.Infrastructure.Services.Payments;

/// <summary>
/// Reads the beneficiary account from configuration. Validation fails soft: an enabled but invalid
/// configuration logs one Error (never the account number) and yields null, so checkout hides the
/// option instead of the site going down. The result is cached per options instance, so it is
/// re-validated (and logged once) whenever the configuration changes.
/// </summary>
internal sealed class ConfiguredBankTransferAccountProvider(
    IOptionsMonitor<BankTransferOptions> monitor,
    ILogger<ConfiguredBankTransferAccountProvider> logger) : IBankTransferAccountProvider
{
    private readonly object _gate = new();
    private BankTransferOptions? _cachedFor;
    private BankTransferAccount? _cachedAccount;

    public BankTransferAccount? GetAccount()
    {
        var options = monitor.CurrentValue;
        lock (_gate)
        {
            if (!ReferenceEquals(_cachedFor, options))
            {
                _cachedAccount = Validate(options);
                _cachedFor = options;
            }

            return _cachedAccount;
        }
    }

    private BankTransferAccount? Validate(BankTransferOptions o)
    {
        if (!o.Enabled)
            return null;

        var bin = o.BankBin?.Trim() ?? "";
        var number = o.AccountNumber?.Trim() ?? "";
        var name = o.AccountName?.Trim() ?? "";
        var bank = o.BankName?.Trim() ?? "";

        var problems = new List<string>();
        if (bin.Length != 6 || !bin.All(char.IsAsciiDigit)) problems.Add("BankBin must be 6 digits");
        if (number.Length is < 6 or > 19 || !number.All(char.IsAsciiDigit)) problems.Add("AccountNumber must be 6-19 digits");
        if (bank.Length == 0) problems.Add("BankName is empty");
        if (name.Length is 0 or > 50 || !name.All(c => c == ' ' || char.IsAsciiLetterUpper(c)))
            problems.Add("AccountName must be 1-50 upper-case ASCII letters and spaces");

        if (problems.Count > 0)
        {
            logger.LogError(
                "Bank transfer is enabled but Payments:BankTransfer is invalid ({Problems}); the option is hidden.",
                string.Join("; ", problems));
            return null;
        }

        var branch = string.IsNullOrWhiteSpace(o.Branch) ? null : o.Branch.Trim();
        return new BankTransferAccount(bin, bank, number, name, branch);
    }
}
