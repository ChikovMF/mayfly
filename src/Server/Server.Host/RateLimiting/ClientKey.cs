using System.Net;
using System.Net.Sockets;

namespace Server.Host.RateLimiting;

internal static class ClientKey
{
    private const int Ipv6PrefixBytes = 8;

    public static string From(IPAddress? address)
    {
        if (address is null)
        {
            return string.Empty;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        var bytes = address.GetAddressBytes();
        Array.Clear(bytes, Ipv6PrefixBytes, bytes.Length - Ipv6PrefixBytes);
        return new IPAddress(bytes).ToString();
    }
}
