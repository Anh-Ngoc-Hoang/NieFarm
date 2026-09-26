namespace NieFarm.Web.Services.Cart;

/// <summary>Constants shared between CartCookieMiddleware, App.razor, CartState and
/// Pages/Account/Login.cshtml.cs.</summary>
public static class CartCookie
{
    public const string Name = "nf_cart";
    public const string HttpContextItemKey = "CartAnonymousId";
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(60);
}
