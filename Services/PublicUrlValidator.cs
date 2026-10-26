using System.Net;
using System.Net.Sockets;

namespace GameStudioSite.Services;

public static class PublicUrlValidator
{
    private static readonly (IPAddress Network, int Prefix)[] BlockedIpv4 =
    {
        (IPAddress.Parse("0.0.0.0"), 8),
        (IPAddress.Parse("10.0.0.0"), 8),
        (IPAddress.Parse("100.64.0.0"), 10),
        (IPAddress.Parse("127.0.0.0"), 8),
        (IPAddress.Parse("169.254.0.0"), 16),
        (IPAddress.Parse("172.16.0.0"), 12),
        (IPAddress.Parse("192.0.0.0"), 24),
        (IPAddress.Parse("192.0.2.0"), 24),
        (IPAddress.Parse("192.88.99.0"), 24),
        (IPAddress.Parse("192.168.0.0"), 16),
        (IPAddress.Parse("198.18.0.0"), 15),
        (IPAddress.Parse("198.51.100.0"), 24),
        (IPAddress.Parse("203.0.113.0"), 24),
        (IPAddress.Parse("224.0.0.0"), 4),
        (IPAddress.Parse("240.0.0.0"), 4)
    };

    public static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return !BlockedIpv4.Any(range =>
                IsInSubnet(address, range.Network, range.Prefix));
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Permit only global-unicast IPv6 space.
            byte[] bytes = address.GetAddressBytes();

            if ((bytes[0] & 0xE0) != 0x20)
                return false;

            // Documentation range: 2001:db8::/32
            if (bytes[0] == 0x20 && bytes[1] == 0x01 &&
                bytes[2] == 0x0D && bytes[3] == 0xB8)
                return false;

            // Special-purpose range: 2001::/23
            if (bytes[0] == 0x20 && bytes[1] == 0x01 &&
                (bytes[2] & 0xFE) == 0x00)
                return false;

            return true;
        }

        return false;
    }

    private static bool IsInSubnet(
        IPAddress address,
        IPAddress network,
        int prefix)
    {
        byte[] addressBytes = address.GetAddressBytes();
        byte[] networkBytes = network.GetAddressBytes();

        int fullBytes = prefix / 8;
        int remainingBits = prefix % 8;

        for (int i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != networkBytes[i])
                return false;
        }

        if (remainingBits == 0)
            return true;

        int mask = 0xFF << (8 - remainingBits);

        return (addressBytes[fullBytes] & mask)
            == (networkBytes[fullBytes] & mask);
    }
}