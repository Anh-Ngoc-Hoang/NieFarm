using System.Globalization;
using NieFarm.Domain.Enums;

namespace NieFarm.Web.Content;

/// <summary>
/// Presentation for the home page's "Vietnam Coffee Stories" map: maps each fixed
/// <see cref="CoffeeRegionKey"/> to its generated SVG geometry (<see cref="VietnamMapGeometry"/>),
/// its CSS slug and its fixed Vietnamese map label. The label is deliberately not an editable
/// column, so an admin can never make the map say something that disagrees with its shape.
/// </summary>
public static class CoffeeRegionUi
{
    public const CoffeeRegionKey DefaultKey = CoffeeRegionKey.South;

    /// <summary>Map order, north to south — also the order of the segment buttons.</summary>
    public static readonly IReadOnlyList<CoffeeRegionKey> All =
        [CoffeeRegionKey.North, CoffeeRegionKey.Central, CoffeeRegionKey.South];

    /// <summary>
    /// Where the "Miền Trung" label sits: offshore, east-south-east of the region's inner point,
    /// because the southern Central strip is too narrow to hold it. The label is left-aligned at
    /// this point and a hairline leader runs back to <see cref="VietnamMapGeometry.CentralLabel"/>.
    /// Chosen by eye to clear the coast and the Hoàng Sa ellipse at 390 px and 1440 px.
    /// </summary>
    public static readonly MapPoint CentralOffshoreLabel =
        new(VietnamMapGeometry.CentralLabel.X + 66, VietnamMapGeometry.CentralLabel.Y + 38);

    /// <summary>
    /// Where the "Tây Nguyên – Nam Bộ" label sits: offshore in the sea east of the Mekong delta, above Côn
    /// Đảo. The region's inner point falls on the narrow neck under Tây Nguyên, and the widest
    /// band of the delta (about 79 viewBox units) is narrower than the label at every width
    /// (about 76 units at 1440 px, 111 at 390 px), so an inland label always crossed the
    /// outline. Left-aligned, with a leader from <see cref="SouthLeaderStart"/>.
    /// </summary>
    public static readonly MapPoint SouthOffshoreLabel = new(222, 668);

    /// <summary>A point inside the delta where the South label's leader starts.</summary>
    public static readonly MapPoint SouthLeaderStart = new(150, 655);

    /// <summary>Hairline leaders from inside a region to its offshore label, in viewBox units.</summary>
    public static readonly IReadOnlyList<(MapPoint From, MapPoint To)> Leaders =
    [
        (VietnamMapGeometry.CentralLabel, new(CentralOffshoreLabel.X - 5, CentralOffshoreLabel.Y)),
        (SouthLeaderStart, new(SouthOffshoreLabel.X - 5, SouthOffshoreLabel.Y))
    ];

    /// <summary>True when the label hangs off its point to the right, at the end of a leader.</summary>
    public static bool IsOffshore(CoffeeRegionKey key) => key is CoffeeRegionKey.Central or CoffeeRegionKey.South;

    public static string Slug(CoffeeRegionKey key) => key switch
    {
        CoffeeRegionKey.North => "north",
        CoffeeRegionKey.Central => "central",
        CoffeeRegionKey.South => "south",
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };

    public static string Label(CoffeeRegionKey key) => key switch
    {
        CoffeeRegionKey.North => "Miền Bắc",
        CoffeeRegionKey.Central => "Miền Trung",
        CoffeeRegionKey.South => "Tây Nguyên – Nam Bộ",
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };

    /// <summary>
    /// The map's own label, broken into lines. "Tây Nguyên – Nam Bộ" is two lines on the map only:
    /// at 13 px on a 390 px phone a single line ran into the Trường Sa ellipse. Chips, the panel
    /// eyebrow and aria labels keep the one-line <see cref="Label"/>.
    /// </summary>
    public static IReadOnlyList<string> MapLabelLines(CoffeeRegionKey key) => key switch
    {
        CoffeeRegionKey.South => ["Tây Nguyên –", "Nam Bộ"],
        _ => [Label(key)]
    };

    public static string Path(CoffeeRegionKey key) => key switch
    {
        CoffeeRegionKey.North => VietnamMapGeometry.NorthPath,
        CoffeeRegionKey.Central => VietnamMapGeometry.CentralPath,
        CoffeeRegionKey.South => VietnamMapGeometry.SouthPath,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };

    /// <summary>Where the region's HTML label is placed (Central and South offshore; see above).</summary>
    public static MapPoint LabelPoint(CoffeeRegionKey key) => key switch
    {
        CoffeeRegionKey.North => VietnamMapGeometry.NorthLabel,
        CoffeeRegionKey.Central => CentralOffshoreLabel,
        CoffeeRegionKey.South => SouthOffshoreLabel,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };

    /// <summary>
    /// "--x:12.34;--y:56.78" — a viewBox point as percentages of the map box, for the absolutely
    /// positioned HTML labels (nf-pages.css ".hmap__label"). Invariant culture, so a vi-VN
    /// request never writes a decimal comma into CSS.
    /// </summary>
    public static string PositionStyle(MapPoint point) =>
        string.Create(CultureInfo.InvariantCulture,
            $"--x:{point.X / VietnamMapGeometry.Width * 100:0.##};--y:{point.Y / VietnamMapGeometry.Height * 100:0.##}");

    /// <summary>Formats a viewBox number for an SVG attribute (invariant culture).</summary>
    public static string Num(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);
}
