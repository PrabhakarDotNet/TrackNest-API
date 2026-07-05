using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    // New: shared fake configuration, since AuthService now needs Google:ClientId
    private IConfiguration CreateFakeConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            { "Google:ClientId", "fake-client-id.apps.googleusercontent.com" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    // ✅ TEST 1: SignupAsync - should return new user Id
    [Fact]
    public async Task SignupAsync_ShouldReturnUserId_WhenUserDoesNotExist()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

        var signupDto = new UserSignupDto
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        };

        var userId = await service.SignupAsync(signupDto);

        userId.Should().BeGreaterThan(0);
    }

    // ✅ TEST 2: SignupAsync - should throw exception if user already exists
    [Fact]
    public async Task SignupAsync_ShouldThrowException_WhenUserAlreadyExists()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        });
        await context.SaveChangesAsync();

        var signupDto = new UserSignupDto
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        };

        var act = async () => await service.SignupAsync(signupDto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User already exists");
    }

    // ✅ TEST 3: LoginAsync - should return tokens when credentials are valid
    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResult_WhenCredentialsAreValid()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

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

        var result = await service.LoginAsync(loginDto);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("fake-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
        result.User.Username.Should().Be("prabhu");
    }

    // ✅ TEST 4: LoginAsync - should return null when password is wrong
    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsWrong()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

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
            Password = "WrongPassword"
        };

        var result = await service.LoginAsync(loginDto);

        result.Should().BeNull();
    }

    // ✅ TEST 5: LoginAsync - should return null when user does not exist
    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

        var loginDto = new UserLoginDto
        {
            Username = "nonexistent",
            Password = "Test@123"
        };

        var result = await service.LoginAsync(loginDto);

        result.Should().BeNull();
    }

    // ✅ TEST 6: RefreshTokenAsync - should return new tokens when refresh token is valid
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        });
        await context.SaveChangesAsync();

        var result = await service.RefreshTokenAsync("valid-refresh-token");

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("fake-access-token");
    }

    // ✅ TEST 7: RefreshTokenAsync - should return null when refresh token is expired
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNull_WhenRefreshTokenIsExpired()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

        context.Users.Add(new User
        {
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123",
            RefreshToken = "expired-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var result = await service.RefreshTokenAsync("expired-refresh-token");

        result.Should().BeNull();
    }

    // ✅ TEST 8: GoogleLoginAsync - should create a new user when email doesn't exist
    // NOTE: This test will NOT actually pass end-to-end, because GoogleJsonWebSignature.ValidateAsync
    // makes a real call to Google's servers to validate the token signature — it cannot be unit tested
    // with a fake token like "fake-id-token" without mocking/wrapping that static call.
    // Consider wrapping GoogleJsonWebSignature.ValidateAsync behind an injectable interface
    // (e.g. IGoogleTokenValidator) if you want this fully unit-testable. Leaving this here as a stub
    // to show intent; see note below the test.
    [Fact(Skip = "Requires wrapping GoogleJsonWebSignature behind an injectable interface to unit test properly")]
    public async Task GoogleLoginAsync_ShouldCreateNewUser_WhenEmailDoesNotExist()
    {
        var context = CreateInMemoryContext();
        var jwtMock = CreateJwtMock();
        var config = CreateFakeConfiguration();
        var service = new AuthService(context, jwtMock.Object, config);

        var request = new GoogleLoginRequestDto
        {
            IdToken = "fake-id-token"
        };

        var result = await service.GoogleLoginAsync(request);

        result.Should().NotBeNull();
        result.User.Email.Should().NotBeNullOrEmpty();
    }
}