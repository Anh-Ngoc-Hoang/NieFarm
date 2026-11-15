namespace NieFarm.Web.Content;

/// <summary>Display helper: converts stored UTC timestamps to Vietnam time (fixed UTC+7, no DST), and back.</summary>
public static class VnTime
{
    public static DateTime FromUtc(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddHours(7);

    /// <summary>Converts a Vietnam-local wall-clock time (e.g. from a datetime-local input) to UTC.</summary>
    public static DateTime ToUtc(DateTime vietnamLocal) =>
        DateTime.SpecifyKind(vietnamLocal.AddHours(-7), DateTimeKind.Utc);
}
