using System.Net;
using Server.Host.RateLimiting;

namespace Server.Tests.Host;

public class ClientKeyTests
{
    [Fact]
    public void From_NullAddress_ReturnsEmpty()
    {
        // Arrange
        // Act
        var key = ClientKey.From(null);

        // Assert
        Assert.Empty(key);
    }

    [Fact]
    public void From_SameIpv4Address_ReturnsSameKey()
    {
        // Arrange
        var first = IPAddress.Parse("203.0.113.7");
        var second = IPAddress.Parse("203.0.113.7");

        // Act
        var firstKey = ClientKey.From(first);
        var secondKey = ClientKey.From(second);

        // Assert
        Assert.Equal(firstKey, secondKey);
    }

    [Fact]
    public void From_DifferentIpv4Addresses_ReturnsDifferentKeys()
    {
        // Arrange
        var first = IPAddress.Parse("203.0.113.7");
        var second = IPAddress.Parse("203.0.113.8");

        // Act
        var firstKey = ClientKey.From(first);
        var secondKey = ClientKey.From(second);

        // Assert
        Assert.NotEqual(firstKey, secondKey);
    }

    [Fact]
    public void From_Ipv4MappedAddress_ReturnsSameKeyAsIpv4()
    {
        // Arrange
        var ipv4 = IPAddress.Parse("203.0.113.7");
        var mapped = ipv4.MapToIPv6();

        // Act
        var ipv4Key = ClientKey.From(ipv4);
        var mappedKey = ClientKey.From(mapped);

        // Assert
        Assert.Equal(ipv4Key, mappedKey);
    }

    [Fact]
    public void From_Ipv6AddressesInSamePrefix_ReturnsSameKey()
    {
        // Arrange
        var first = IPAddress.Parse("2001:db8:1:2::1");
        var second = IPAddress.Parse("2001:db8:1:2:aaaa:bbbb:cccc:dddd");

        // Act
        var firstKey = ClientKey.From(first);
        var secondKey = ClientKey.From(second);

        // Assert
        Assert.Equal(firstKey, secondKey);
    }

    [Fact]
    public void From_Ipv6AddressesInDifferentPrefixes_ReturnsDifferentKeys()
    {
        // Arrange
        var first = IPAddress.Parse("2001:db8:1:2::1");
        var second = IPAddress.Parse("2001:db8:1:3::1");

        // Act
        var firstKey = ClientKey.From(first);
        var secondKey = ClientKey.From(second);

        // Assert
        Assert.NotEqual(firstKey, secondKey);
    }
}
