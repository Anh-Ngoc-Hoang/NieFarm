using System.Globalization;

namespace NieFarm.Web.Content;

/// <summary>
/// Geometry of the sensory radar chart on the traceability page: plain SVG numbers computed on the server, no
/// script. Values are 1 to 5 on <see cref="Axis"/> count axes (3 or more), the first axis pointing straight up and the
/// rest clockwise. The page pairs it with a text table for screen readers.
/// </summary>
public sealed class TraceRadar
{
    public const double Size = 260;
    public const double Radius = 84;
    public const int Max = 5;

    public sealed record Axis(string Label, int Value, string TickX, string TickY, string DotX, string DotY,
        string LabelX, string LabelY, string Anchor);

    public IReadOnlyList<Axis> Axes { get; }

    /// <summary>One polygon (points attribute) per grid ring, from level 1 to 5.</summary>
    public IReadOnlyList<string> Rings { get; }

    public string Shape { get; }

    public double Center => Size / 2;

    private TraceRadar(IReadOnlyList<Axis> axes, IReadOnlyList<string> rings, string shape)
    {
        Axes = axes;
        Rings = rings;
        Shape = shape;
    }

    /// <summary>Null when fewer than three scales are present (the page then draws thin bars instead).</summary>
    public static TraceRadar? Create(IReadOnlyList<(string Label, int Value)> scales)
    {
        if (scales.Count < 3)
            return null;

        var n = scales.Count;
        var c = Size / 2;

        static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        (double X, double Y) At(int index, double radius)
        {
            var angle = -Math.PI / 2 + 2 * Math.PI * index / n;
            return (c + radius * Math.Cos(angle), c + radius * Math.Sin(angle));
        }

        var rings = new List<string>();
        for (var level = 1; level <= Max; level++)
        {
            var r = Radius * level / Max;
            rings.Add(string.Join(' ', Enumerable.Range(0, n).Select(i =>
            {
                var (x, y) = At(i, r);
                return $"{F(x)},{F(y)}";
            })));
        }

        var axes = new List<Axis>();
        var shape = new List<string>();
        for (var i = 0; i < n; i++)
        {
            var (tx, ty) = At(i, Radius);
            var (dx, dy) = At(i, Radius * scales[i].Value / Max);
            var (lx, ly) = At(i, Radius + 20);
            shape.Add($"{F(dx)},{F(dy)}");

            var anchor = Math.Abs(lx - c) < 4 ? "middle" : lx > c ? "start" : "end";
            axes.Add(new Axis(scales[i].Label, scales[i].Value, F(tx), F(ty), F(dx), F(dy), F(lx), F(ly + 4), anchor));
        }

        return new TraceRadar(axes, rings, string.Join(' ', shape));
    }
}
