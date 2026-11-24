using System.Globalization;
using NieFarm.Domain.Enums;

namespace NieFarm.Web.Content;

/// <summary>
/// Vietnamese labels and vi-VN number/date formatting for the public traceability page (R-26). Presentation only:
/// which blocks exist is decided by the Application DTO, never here.
/// </summary>
public static class TraceText
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", Vi);

    public static string DateRange(DateOnly? from, DateOnly? to) => (from, to) switch
    {
        ({ } f, { } t) when f == t => Date(f),
        ({ } f, { } t) => $"{Date(f)} – {Date(t)}",
        ({ } f, null) => Date(f),
        (null, { } t) => Date(t),
        _ => string.Empty
    };

    /// <summary>"Ngon nhất từ … đến …", from only, or to only. Null when there is no window.</summary>
    public static string? PeakWindow(DateOnly? from, DateOnly? to) => (from, to) switch
    {
        ({ } f, { } t) => $"Ngon nhất từ {Date(f)} đến {Date(t)}",
        ({ } f, null) => $"Ngon nhất từ {Date(f)}",
        (null, { } t) => $"Ngon nhất đến {Date(t)}",
        _ => null
    };

    /// <summary>1.450 or 1.450 – 1.600 (no unit).</summary>
    public static string AltitudeNumber(int? min, int? max) => (min, max) switch
    {
        ({ } a, { } b) when a != b => $"{a.ToString("N0", Vi)} – {b.ToString("N0", Vi)}",
        ({ } a, { }) => a.ToString("N0", Vi),
        ({ } a, null) => a.ToString("N0", Vi),
        (null, { } b) => b.ToString("N0", Vi),
        _ => string.Empty
    };

    public static string Altitude(int? min, int? max) => AltitudeNumber(min, max) + " m";

    /// <summary>84,25 (trailing zeros dropped, comma decimal).</summary>
    public static string Score(decimal score) => score.ToString("0.##", Vi);

    public static string Percent(decimal value) => value.ToString("0.#", Vi) + "%";

    public static string Weight(int grams) => grams >= 1000 && grams % 100 == 0
        ? (grams / 1000m).ToString("0.#", Vi) + " kg"
        : grams.ToString("N0", Vi) + " g";

    public static string ProcessingLabel(CoffeeProcessingMethod method) => method switch
    {
        CoffeeProcessingMethod.Washed => "Chế biến ướt (Washed)",
        CoffeeProcessingMethod.Natural => "Chế biến khô (Natural)",
        CoffeeProcessingMethod.Honey => "Chế biến mật ong (Honey)",
        CoffeeProcessingMethod.SemiWashed => "Chế biến bán ướt (Semi-washed)",
        CoffeeProcessingMethod.Anaerobic => "Lên men yếm khí (Anaerobic)",
        _ => "Phương pháp sơ chế khác"
    };

    /// <summary>A one-word form for the highlight strip.</summary>
    public static string ProcessingShort(CoffeeProcessingMethod method) => method switch
    {
        CoffeeProcessingMethod.Washed => "Ướt",
        CoffeeProcessingMethod.Natural => "Khô",
        CoffeeProcessingMethod.Honey => "Honey",
        CoffeeProcessingMethod.SemiWashed => "Bán ướt",
        CoffeeProcessingMethod.Anaerobic => "Yếm khí",
        _ => "Khác"
    };

    public static string RoastLabel(RoastLevel level) => "Rang " + RoastShort(level).ToLower(Vi);

    public static string RoastShort(RoastLevel level) => level switch
    {
        RoastLevel.Light => "Nhạt",
        RoastLevel.MediumLight => "Vừa nhạt",
        RoastLevel.Medium => "Vừa",
        RoastLevel.MediumDark => "Vừa đậm",
        _ => "Đậm"
    };

    public static string BrewLabel(BrewMethod method) => method switch
    {
        BrewMethod.Phin => "Phin",
        BrewMethod.Espresso => "Espresso",
        BrewMethod.PourOver => "Pour-over (V60)",
        BrewMethod.ColdBrew => "Cold brew",
        BrewMethod.FrenchPress => "French press",
        _ => "Moka pot"
    };

    public static string Fermentation(int hours) => $"Lên men {hours.ToString("N0", Vi)} giờ";

    public static string Drying(int days) => $"Phơi {days.ToString("N0", Vi)} ngày";
}
