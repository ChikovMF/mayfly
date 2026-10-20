using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Server.Host.Endpoints;
using Server.Host.RateLimiting;

namespace Server.Tests.Host;

public class PasswordRateLimiterPolicyTests
{
    private const int TokenLimit = 3;

    private readonly PasswordRateLimiterPolicy _policy = new(Options.Create(new PasswordRateLimitOptions
    {
        TokenLimit = TokenLimit,
        TokensPerPeriod = 1,
        ReplenishmentPeriod = TimeSpan.FromHours(1),
    }));

    [Fact]
    public void GetPartition_WithoutPasswordHeader_NeverLimits()
    {
        // Arrange
        var context = CreateContext("203.0.113.7", withPassword: false);

        // Act
        var partition = _policy.GetPartition(context);

        // Assert
        using var limiter = partition.Factory(partition.PartitionKey);
        for (var i = 0; i < TokenLimit * 10; i++)
        {
            Assert.True(limiter.AttemptAcquire().IsAcquired);
        }
    }

    [Fact]
    public void GetPartition_WithPasswordHeader_RejectsAfterTokenLimit()
    {
        // Arrange
        var context = CreateContext("203.0.113.7", withPassword: true);

        // Act
        var partition = _policy.GetPartition(context);

        // Assert
        using var limiter = partition.Factory(partition.PartitionKey);
        for (var i = 0; i < TokenLimit; i++)
        {
            Assert.True(limiter.AttemptAcquire().IsAcquired);
        }

        Assert.False(limiter.AttemptAcquire().IsAcquired);
    }

    [Fact]
    public void GetPartition_WithPasswordHeaderFromSameClient_ReturnsSamePartitionKey()
    {
        // Arrange
        var first = CreateContext("203.0.113.7", withPassword: true);
        var second = CreateContext("203.0.113.7", withPassword: true);

        // Act
        var firstPartition = _policy.GetPartition(first);
        var secondPartition = _policy.GetPartition(second);

        // Assert
        Assert.Equal(firstPartition.PartitionKey, secondPartition.PartitionKey);
    }

    [Fact]
    public void GetPartition_WithPasswordHeaderFromDifferentClients_ReturnsDifferentPartitionKeys()
    {
        // Arrange
        var first = CreateContext("203.0.113.7", withPassword: true);
        var second = CreateContext("203.0.113.8", withPassword: true);

        // Act
        var firstPartition = _policy.GetPartition(first);
        var secondPartition = _policy.GetPartition(second);

        // Assert
        Assert.NotEqual(firstPartition.PartitionKey, secondPartition.PartitionKey);
    }

    private static DefaultHttpContext CreateContext(string address, bool withPassword)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        if (withPassword)
        {
            context.Request.Headers[FilesEndpoints.PasswordHeader] = "cGFzc3dvcmQ=";
        }

        return context;
    }
}
