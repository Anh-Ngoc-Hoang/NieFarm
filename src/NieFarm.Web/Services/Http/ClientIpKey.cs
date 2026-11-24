using System.Net;
using System.Net.Sockets;

namespace NieFarm.Web.Services.Http;

/// <summary>
/// The key a client is rate limited and de-duplicated by (R-26 D16/D11): the IPv4 address, or the /64 prefix of an
/// IPv6 address (a single subscriber owns a whole /64, so per-address keys would be free to rotate). The address is
/// the one after ForwardedHeaders middleware, so it is only the real client when <c>ReverseProxy</c> is configured.
/// Never logged and never stored.
/// </summary>
public static class ClientIpKey
{
    public const string Unknown = "unknown";

    public static string For(HttpContext context) => For(context.Connection.RemoteIpAddress);

    public static string For(IPAddress? address)
    {
        if (address is null)
            return Unknown;

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
            return address.ToString();

        var bytes = address.GetAddressBytes();
        Array.Clear(bytes, 8, 8);
        return new IPAddress(bytes) + "/64";
    }
}
