using System.Globalization;

namespace NieFarm.Web.Content;

/// <summary>Currency formatting shared by the public site and the admin.</summary>
public static class Money
{
    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");

    /// <summary>Formats an amount as Vietnamese đồng, e.g. <c>185.000 ₫</c>.</summary>
    public static string Vnd(decimal amount) =>
        string.Concat(amount.ToString("#,##0", Vietnamese), " ₫");
}
