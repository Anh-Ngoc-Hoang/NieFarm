using System.Reflection;
using PdfSharp.Fonts;

namespace NieFarm.Web.Services.Traceability;

/// <summary>
/// Supplies Be Vietnam Pro (SIL OFL 1.1, embedded in the assembly) to PDFsharp. Vietnamese needs the
/// precomposed (NFC) glyphs the font ships with; there is no shaping engine behind PDFsharp, so callers
/// normalise text to FormC before drawing.
/// </summary>
public sealed class EmbeddedFontResolver : IFontResolver
{
    public const string FamilyName = "Be Vietnam Pro";

    private const string RegularFace = "BeVietnamPro-Regular";
    private const string SemiBoldFace = "BeVietnamPro-SemiBold";

    private static readonly Assembly Assembly = typeof(EmbeddedFontResolver).Assembly;

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Any family name maps onto the one embedded family; italic is not shipped, so it falls back to upright.
        _ = familyName;
        _ = isItalic;
        return new FontResolverInfo(isBold ? SemiBoldFace : RegularFace);
    }

    public byte[]? GetFont(string faceName)
    {
        if (faceName is not (RegularFace or SemiBoldFace))
            return null;

        using var stream = Assembly.GetManifestResourceStream($"NieFarm.Web.Fonts.{faceName}.ttf")
            ?? throw new InvalidOperationException($"Embedded font '{faceName}' is missing from the assembly.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
