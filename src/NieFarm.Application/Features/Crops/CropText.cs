using System.Text.RegularExpressions;

namespace NieFarm.Application.Features.Crops;

/// <summary>Splits Crop's plain-text fields into display lists (D5 — no MarkupString, no sanitiser).</summary>
public static partial class CropText
{
    /// <summary>Splits on a blank line (one or more), trims each paragraph, drops empty ones.</summary>
    public static IReadOnlyList<string> SplitParagraphs(string story)
    {
        if (string.IsNullOrWhiteSpace(story))
            return [];

        return ParagraphSplitRegex().Split(story.Trim())
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();
    }

    /// <summary>Splits a comma list, trims each note, drops empty ones. Assumes Crop.Update already normalised it.</summary>
    public static IReadOnlyList<string> SplitTastingNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return [];

        return notes.Split(',')
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .ToList();
    }

    [GeneratedRegex(@"\r?\n\s*\r?\n")]
    private static partial Regex ParagraphSplitRegex();
}
