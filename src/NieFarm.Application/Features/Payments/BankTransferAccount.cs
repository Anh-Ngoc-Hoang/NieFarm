namespace NieFarm.Application.Features.Payments;

/// <summary>The beneficiary account customers transfer to. BankBin is the 6-digit NAPAS acquirer BIN.</summary>
public record BankTransferAccount(
    string BankBin, string BankName, string AccountNumber, string AccountName, string? Branch);
