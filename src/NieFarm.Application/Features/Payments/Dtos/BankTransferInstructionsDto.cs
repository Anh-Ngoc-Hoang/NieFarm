namespace NieFarm.Application.Features.Payments.Dtos;

/// <summary>Everything a customer needs to pay one bank-transfer order, plus the VietQR payload to render.</summary>
public record BankTransferInstructionsDto(
    string BankName, string AccountNumber, string AccountName, string? Branch,
    decimal Amount, string TransferContent, string QrPayload);
