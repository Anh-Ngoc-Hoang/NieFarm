using QRCoder;

namespace NieFarm.Web.Content;

/// <summary>
/// Renders a payload into a PNG data URI, locally. The image decides where the customer money goes,
/// so it is never rendered by a third party. PngByteQRCode has no System.Drawing dependency.
/// </summary>
public static class QrCodeImage
{
    // verify: QRCoder 1.8.1 API (QRCodeGenerator.CreateQrCode + PngByteQRCode.GetGraphic) as in its wiki;
    // compiled but not scan-tested offline (plan step 12).
    public static string ToPngDataUri(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data);
        var bytes = png.GetGraphic(8);
        return "data:image/png;base64," + Convert.ToBase64String(bytes);
    }
}
