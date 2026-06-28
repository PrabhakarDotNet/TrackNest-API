using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;
using TrackNest.Infrastructure.Services;

namespace TrackNest.Tests;

public class AuthServiceTests
{
    private TrackNestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TrackNestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TrackNestDbContext(options);
    }

    private Mock<IJwtTokenService> CreateJwtMock()
    {
        var mock = new Mock<IJwtTokenService>();
        mock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("fake-access-token");
        mock.Setup(x => x.GenerateRefreshToken()).Returns("fake-refresh-token");
        return mock;
    }

    // ✅ TEST 1: SignupAsync - should return new user Id
    [Fact]
    public async Task SignupAsync_ShouldReturnUserId_WhenUserDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var service = new AuthService(context, jwtMock.Object);

        var signupDto = new UserSignupDto
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        };

        // Act
        var userId = await service.SignupAsync(signupDto);

        // Assert
        userId.Should().BeGreaterThan(0);
    }

    // ✅ TEST 2: SignupAsync - should throw exception if user already exists
    [Fact]
    public async Task SignupAsync_ShouldThrowException_WhenUserAlreadyExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var service = new AuthService(context, jwtMock.Object);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        });
        await context.SaveChangesAsync();

        var signupDto = new UserSignupDto
        {
            Username = "prabhu", // same username
            Email = "prabhu@test.com",
            Password = "Test@123"
        };

        // Act
        var act = async () => await service.SignupAsync(signupDto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User already exists");
    }

    // ✅ TEST 3: LoginAsync - should return tokens when credentials are valid
    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResult_WhenCredentialsAreValid()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var service = new AuthService(context, jwtMock.Object);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        });
        await context.SaveChangesAsync();

        var loginDto = new UserLoginDto
        {
            Username = "prabhu",
            Password = "Test@123"
        };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("fake-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
        result.User.Username.Should().Be("prabhu");
    }

    // ✅ TEST 4: LoginAsync - should return null when password is wrong
    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsWrong()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var service = new AuthService(context, jwtMock.Object);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        });
        await context.SaveChangesAsync();

        var loginDto = new UserLoginDto
        {
            Username = "prabhu",
            Password = "WrongPassword" // wrong
        };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    // ✅ TEST 5: LoginAsync - should return null when user does not exist
    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var service = new AuthService(context, jwtMock.Object);

        var loginDto = new UserLoginDto
        {
            Username = "nonexistent",
            Password = "Test@123"
        };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    // ✅ TEST 6: RefreshTokenAsync - should return new tokens when refresh token is valid
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var service = new AuthService(context, jwtMock.Object);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7) // not expired
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.RefreshTokenAsync("valid-refresh-token");

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("fake-access-token");
    }

    // ✅ TEST 7: RefreshTokenAsync - should return null when refresh token is expired
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNull_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var service = new AuthService(context, jwtMock.Object);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123",
            RefreshToken = "expired-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // already expired
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.RefreshTokenAsync("expired-refresh-token");

        // Assert
        result.Should().BeNull();
    }
}