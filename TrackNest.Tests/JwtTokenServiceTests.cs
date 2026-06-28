using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Services;

namespace TrackNest.Tests;

public class JwtTokenServiceTests
{
    private JwtTokenService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsASecretKeyForTrackNestTesting123!" },
                { "Jwt:Issuer", "TrackNest" },
                { "Jwt:Audience", "TrackNestUsers" },
                { "Jwt:ExpiryMinutes", "60" }
            })
            .Build();

        return new JwtTokenService(config);
    }

    // ✅ TEST 1: GenerateToken - should return non-empty token string
    [Fact]
    public void GenerateToken_ShouldReturnToken_WhenUserIsValid()
    {
        // Arrange
        var service = CreateService();
        var user = new User { Id = 1, Username = "prabhu", Email = "prabhu@test.com" };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    // ✅ TEST 2: GenerateToken - should contain 3 JWT parts (header.payload.signature)
    [Fact]
    public void GenerateToken_ShouldBeValidJwtFormat()
    {
        // Arrange
        var service = CreateService();
        var user = new User { Id = 1, Username = "prabhu", Email = "prabhu@test.com" };

        // Act
        var token = service.GenerateToken(user);
        var parts = token.Split('.');

        // Assert
        parts.Length.Should().Be(3);
    }

    // ✅ TEST 3: GenerateRefreshToken - should return non-empty string
    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Arrange
        var service = CreateService();

        // Act
        var refreshToken = service.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    // ✅ TEST 4: GenerateRefreshToken - each call should return unique token
    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokenEachTime()
    {
        // Arrange
        var service = CreateService();

        // Act
        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }
}