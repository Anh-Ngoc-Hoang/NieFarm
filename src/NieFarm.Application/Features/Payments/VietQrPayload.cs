using System.Globalization;
using System.Text;

namespace NieFarm.Application.Features.Payments;

/// <summary>
/// Pure builder for the NAPAS VietQR (EMVCo merchant-presented, "transfer to account") payload.
/// No dependencies, so it can be checked in isolation. The text is rendered into a QR image in Web.
/// <para>
/// Layout (TLV: 2-digit tag, 2-digit length, value):
/// 00=01, 01=12 (dynamic: an amount is present), 38={00=A000000727, 01={00=BIN, 01=account}, 02=QRIBFTTA},
/// 53=704, 54=amount, 58=VN, 62={08=content}, 63=CRC16.
/// </para>
/// </summary>
public static class VietQrPayload
{
    public const int MaxContentLength = 25;

    // verify: the tag layout follows the NAPAS VietQR spec as recalled offline (it matches the widely
    // published example 00020101021238570010A000000727...0208QRIBFTTA53037045802VN...6304xxxx);
    // confirm by scanning with real banking apps (plan step 12).
    public static string Build(string bankBin, string accountNumber, decimal amount, string content)
    {
        if (string.IsNullOrWhiteSpace(bankBin) || bankBin.Length != 6 || !bankBin.All(char.IsAsciiDigit))
            throw new ArgumentException("The bank BIN must be 6 digits.", nameof(bankBin));
        if (string.IsNullOrWhiteSpace(accountNumber) || !accountNumber.All(char.IsAsciiLetterOrDigit))
            throw new ArgumentException("The account number must be letters and digits only.", nameof(accountNumber));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be positive.");
        if (string.IsNullOrEmpty(content) || content.Length > MaxContentLength || !content.All(c => c >= 0x20 && c <= 0x7E))
            throw new ArgumentException($"The content must be 1-{MaxContentLength} printable ASCII characters.", nameof(content));

        // Whole dong, rounded up so the customer never underpays.
        var dong = decimal.Ceiling(amount).ToString("0", CultureInfo.InvariantCulture);

        var beneficiary = Tlv("00", bankBin) + Tlv("01", accountNumber);
        var merchantAccount = Tlv("00", "A000000727") + Tlv("01", beneficiary) + Tlv("02", "QRIBFTTA");

        var body = new StringBuilder()
            .Append(Tlv("00", "01"))
            .Append(Tlv("01", "12"))
            .Append(Tlv("38", merchantAccount))
            .Append(Tlv("53", "704"))
            .Append(Tlv("54", dong))
            .Append(Tlv("58", "VN"))
            .Append(Tlv("62", Tlv("08", content)))
            .Append("6304")
            .ToString();

        return body + Crc16CcittFalse(body).ToString("X4", CultureInfo.InvariantCulture);
    }

    private static string Tlv(string tag, string value) =>
        tag + value.Length.ToString("00", CultureInfo.InvariantCulture) + value;

    /// <summary>CRC16/CCITT-FALSE: poly 0x1021, init 0xFFFF, no reflection, no final xor. Check value of "123456789" is 0x29B1.</summary>
    internal static ushort Crc16CcittFalse(string text)
    {
        ushort crc = 0xFFFF;
        foreach (var b in Encoding.ASCII.GetBytes(text))
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
                crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
        }
        return crc;
    }
}
