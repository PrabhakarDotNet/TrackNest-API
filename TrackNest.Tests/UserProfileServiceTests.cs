using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TrackNest.Application.DTOs;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;
using TrackNest.Infrastructure.Services;

namespace TrackNest.Tests;

public class UserProfileServiceTests
{
    private TrackNestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TrackNestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TrackNestDbContext(options);
    }

    // ✅ TEST 1: GetProfileAsync - should return profile when user exists
    [Fact]
    public async Task GetProfileAsync_ShouldReturnProfile_WhenUserExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserProfileService(context);

        context.Users.Add(new User
        {
            Id = 1,
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetProfileAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("prabhu");
        result.Email.Should().Be("prabhu@test.com");
    }

    // ✅ TEST 2: GetProfileAsync - should return null when user does not exist
    [Fact]
    public async Task GetProfileAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserProfileService(context);

        // Act
        var result = await service.GetProfileAsync(99);

        // Assert
        result.Should().BeNull();
    }

    // ✅ TEST 3: UpdateProfileAsync - should update username and email
    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserProfileService(context);

        context.Users.Add(new User
        {
            Id = 1,
            Username = "prabhu",
            Email = "prabhu@test.com",
            Password = "Test@123"
        });
        await context.SaveChangesAsync();

        var updatedProfile = new UserProfileDto
        {
            Id = 1,
            Username = "prabhu_updated",
            Email = "updated@test.com"
        };

        // Act
        await service.UpdateProfileAsync(updatedProfile);

        // Assert
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == 1);
        user!.Username.Should().Be("prabhu_updated");
        user.Email.Should().Be("updated@test.com");
    }

    // ✅ TEST 4: UpdateProfileAsync - should throw exception when user does not exist
    [Fact]
    public async Task UpdateProfileAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserProfileService(context);

        var updatedProfile = new UserProfileDto
        {
            Id = 99,
            Username = "ghost",
            Email = "ghost@test.com"
        };

        // Act
        var act = async () => await service.UpdateProfileAsync(updatedProfile);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User not found");
    }
}