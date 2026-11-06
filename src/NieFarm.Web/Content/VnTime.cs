namespace NieFarm.Web.Content;

/// <summary>Display helper: converts stored UTC timestamps to Vietnam time (fixed UTC+7, no DST).</summary>
public static class VnTime
{
    public static DateTime FromUtc(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddHours(7);
}
