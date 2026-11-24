using System.Text;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;
using NieFarm.Web.Content;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace NieFarm.Web.Services.Traceability;

public enum LabelLayout
{
    /// <summary>One 50 x 30 mm label per page (a thermal or sticker roll printer).</summary>
    Single,

    /// <summary>A4 sheet of 3 x 7 labels of 63.5 x 38.1 mm (the common L7160 stock).</summary>
    A4,
}

/// <summary>
/// Draws the printable batch label (R-26): the QR as vector rectangles (sharp at any printer resolution), the product,
/// the batch code, roast date and weight, and the short address a person can type. Vietnamese needs precomposed glyphs
/// and PDFsharp has no shaping engine, so every string is normalised to FormC before it is measured or drawn, and the
/// embedded Be Vietnam Pro font (see <see cref="EmbeddedFontResolver"/>) supplies the diacritics.
/// </summary>
public static class RoastBatchLabelPdf
{
    public const int MaxCopies = 420;
    public const int MaxSkip = 20;

    private const double Mm = 72.0 / 25.4;

    // The y passed to DrawString is the text baseline.
    private static readonly XStringFormat Baseline = new() { Alignment = XStringAlignment.Near, LineAlignment = XLineAlignment.BaseLine };

    // L7160 geometry (mm), the numbers of plan D22: 3 columns x 7 rows, labels 63.5 x 38.1, pitch 66.04 x 38.1,
    // margins 7.25 (left) / 15.15 (top). Check against the decal stock actually bought (plan Q3).
    private const int SheetColumns = 3;
    private const int SheetRows = 7;
    private const double SheetLabelWidth = 63.5;
    private const double SheetLabelHeight = 38.1;
    private const double SheetPitchX = 66.04;
    private const double SheetMarginLeft = 7.25;
    private const double SheetMarginTop = 15.15;

    private const double SingleWidth = 50.0;
    private const double SingleHeight = 30.0;

    public static byte[] Render(RoastBatchLabelDto batch, string url, string shortText, LabelLayout layout, int copies, int skip)
    {
        copies = Math.Clamp(copies, 1, MaxCopies);
        skip = Math.Clamp(skip, 0, MaxSkip);

        var qr = QrMatrix.Create(url);
        using var document = new PdfDocument();
        document.Info.Title = Text($"Nhãn lô {batch.CodeDisplay}");
        document.Info.Creator = "NieFarm";

        if (layout == LabelLayout.Single)
        {
            for (var i = 0; i < copies; i++)
            {
                var page = document.AddPage();
                page.Width = XUnit.FromMillimeter(SingleWidth);
                page.Height = XUnit.FromMillimeter(SingleHeight);
                using var gfx = XGraphics.FromPdfPage(page);
                DrawLabel(gfx, 0, 0, SingleWidth, SingleHeight, 1.0, batch, qr, shortText);
            }
        }
        else
        {
            const int perSheet = SheetColumns * SheetRows;
            var slot = skip;
            PdfPage? page = null;
            XGraphics? gfx = null;
            try
            {
                for (var i = 0; i < copies; i++, slot++)
                {
                    if (page is null || slot % perSheet == 0)
                    {
                        gfx?.Dispose();
                        page = document.AddPage();
                        page.Width = XUnit.FromMillimeter(210);
                        page.Height = XUnit.FromMillimeter(297);
                        gfx = XGraphics.FromPdfPage(page);
                    }

                    var cell = slot % perSheet;
                    var x = SheetMarginLeft + (cell % SheetColumns) * SheetPitchX;
                    var y = SheetMarginTop + (cell / SheetColumns) * SheetLabelHeight;
                    DrawLabel(gfx!, x, y, SheetLabelWidth, SheetLabelHeight, 1.15, batch, qr, shortText);
                }
            }
            finally
            {
                gfx?.Dispose();
            }
        }

        using var output = new MemoryStream();
        document.Save(output, closeStream: false);
        return output.ToArray();
    }

    private static void DrawLabel(
        XGraphics gfx, double xMm, double yMm, double widthMm, double heightMm, double scale,
        RoastBatchLabelDto batch, QrMatrix qr, string shortText)
    {
        var margin = heightMm * 0.09;
        var qrSide = heightMm * 0.62;
        var qrX = xMm + margin;
        var qrY = yMm + (heightMm - qrSide) / 2;

        // QR: every dark run is one rectangle, filled in a single path so no hairline seams appear between rows.
        var module = qrSide * Mm / qr.Size;
        var path = new XGraphicsPath();
        foreach (var (row, column, length) in qr.Runs())
            path.AddRectangle(new XRect(qrX * Mm + column * module, qrY * Mm + row * module, length * module, module));
        gfx.DrawPath(XBrushes.Black, path);

        var textX = (qrX + qrSide + margin * 0.9) * Mm;
        var textWidth = (xMm + widthMm - margin * 0.6) * Mm - textX;
        var top = (yMm + margin) * Mm;
        var bottom = (yMm + heightMm - margin * 0.8) * Mm;

        // Plan D22 content: NieFarm, product (max 2 lines), "Lô {code}", "Rang {date}" (+ weight), "Quét mã để xem nguồn
        // gốc" and the short address. Everything is stacked top-down; when the column is too short (the 50 x 30 mm
        // label) every font is scaled down together, never below 78 percent. Only if that is still not enough is the
        // "Quét mã..." line dropped, because the QR itself already says it and the short address stays printed.
        var lines = new List<LabelLine>
        {
            new("NieFarm", 5.4 * scale, true, 0),
        };
        var nameFont = new XFont(EmbeddedFontResolver.FamilyName, 6.6 * scale, XFontStyleEx.Bold);
        foreach (var line in WrapTwoLines(gfx, Text(batch.ProductName), nameFont, textWidth))
            lines.Add(new(line, 6.6 * scale, true, 0.12));

        lines.Add(new(Text($"Lô {batch.CodeDisplay}"), 7.2 * scale, true, 0.4));

        var rang = $"Rang {TraceText.Date(batch.RoastDate)}";
        if (batch.NetWeightGrams is { } grams)
            rang += $" · {TraceText.Weight(grams)}";
        lines.Add(new(Text(rang), 5.8 * scale, false, 0.3));

        var scanLine = new LabelLine(Text("Quét mã để xem nguồn gốc"), 4.7 * scale, false, 0.5);
        lines.Add(scanLine);
        lines.Add(new(Text(shortText), 4.7 * scale, false, 0.1));

        var available = bottom - top;
        var factor = FitFactor(lines, available);
        if (factor < MinTextScale)
        {
            lines.Remove(scanLine);
            factor = FitFactor(lines, available);
        }

        factor = Math.Clamp(factor, 0.5, 1.0);

        var y = top;
        foreach (var line in lines)
        {
            var size = line.Size * factor;
            var font = FitFont(gfx, line.Text, size, textWidth, line.Bold ? XFontStyleEx.Bold : XFontStyleEx.Regular);
            y += size * line.GapBefore + size;
            gfx.DrawString(Fit(gfx, line.Text, font, textWidth), font, XBrushes.Black, new XPoint(textX, y), Baseline);
            y += size * 0.2;
        }
    }

    private const double MinTextScale = 0.78;

    private sealed record LabelLine(string Text, double Size, bool Bold, double GapBefore);

    /// <summary>The factor (at most 1) that makes the stacked lines fit the available height.</summary>
    private static double FitFactor(IReadOnlyList<LabelLine> lines, double available)
    {
        var total = lines.Sum(l => l.Size * (l.GapBefore + 1.2));
        return total <= available ? 1.0 : available / total;
    }

    /// <summary>The largest font (not above <paramref name="size"/>, not below 4.5 pt) in which the text fits the column.</summary>
    private static XFont FitFont(XGraphics gfx, string text, double size, double width, XFontStyleEx style)
    {
        var font = new XFont(EmbeddedFontResolver.FamilyName, size, style);
        while (size > 4.5 && gfx.MeasureString(text, font).Width > width)
        {
            size -= 0.2;
            font = new XFont(EmbeddedFontResolver.FamilyName, size, style);
        }

        return font;
    }

    private static string Text(string value) => value.Normalize(NormalizationForm.FormC);

    /// <summary>Greedy word wrap into at most two lines; the second line ends in an ellipsis when it still overflows.</summary>
    private static IEnumerable<string> WrapTwoLines(XGraphics gfx, string text, XFont font, double width)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var first = new StringBuilder();
        var index = 0;
        while (index < words.Length)
        {
            var candidate = first.Length == 0 ? words[index] : first + " " + words[index];
            if (first.Length > 0 && gfx.MeasureString(candidate, font).Width > width)
                break;

            first.Clear().Append(candidate);
            index++;
        }

        // A single word wider than the column is cut by characters.
        yield return Fit(gfx, first.ToString(), font, width);

        if (index >= words.Length)
            yield break;

        yield return Fit(gfx, string.Join(' ', words.Skip(index)), font, width);
    }

    /// <summary>Returns the text, shortened with an ellipsis until it measures no wider than <paramref name="width"/>.</summary>
    private static string Fit(XGraphics gfx, string text, XFont font, double width)
    {
        if (gfx.MeasureString(text, font).Width <= width)
            return text;

        var cut = text;
        while (cut.Length > 1)
        {
            cut = cut[..^1];
            var candidate = cut.TrimEnd() + "…";
            if (gfx.MeasureString(candidate, font).Width <= width)
                return candidate;
        }

        return "…";
    }
}
