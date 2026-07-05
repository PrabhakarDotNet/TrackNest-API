using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using TrackNest.Application.DTOs;
using TrackNest.Application.Events;
using TrackNest.Application.Interfaces;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;
using TrackNest.Infrastructure.Services;

namespace TrackNest.Tests;

// Test double — records published events instead of hitting real RabbitMQ
public class FakeMessagePublisher : IMessagePublisher
{
    public List<object> PublishedMessages { get; } = new();

    public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
    {
        PublishedMessages.Add(message!);
        return Task.CompletedTask;
    }
}

// Test double — simple in-memory implementation of IDistributedCache
public class FakeDistributedCache : IDistributedCache
{
    private readonly Dictionary<string, byte[]> _store = new();

    public byte[]? Get(string key) => _store.TryGetValue(key, out var value) ? value : null;

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        => Task.FromResult(Get(key));

    public void Refresh(string key) { }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key) => _store.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _store[key] = value;

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }
}

public class ExpenseServiceTests
{
    private TrackNestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TrackNestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // fresh DB per test
            .Options;

        return new TrackNestDbContext(options);
    }

    // ✅ TEST 1: CreateAsync - should return a valid expense Id
    [Fact]
    public async Task CreateAsync_ShouldReturnExpenseId_WhenValidRequest()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var publisher = new FakeMessagePublisher();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance, publisher, new FakeDistributedCache());

        var request = new CreateExpenseDto
        {
            Amount = 500,
            Category = "Food",
            Description = "Lunch",
            ExpenseDate = DateTime.UtcNow
        };

        // Act
        var id = await service.CreateAsync(request, userId: 1);

        // Assert
        id.Should().BeGreaterThan(0);
        publisher.PublishedMessages.Should().ContainSingle();
    }

    // ✅ TEST 2: GetByIdAsync - should return expense for correct user
    [Fact]
    public async Task GetByIdAsync_ShouldReturnExpense_WhenUserOwnsIt()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance, new FakeMessagePublisher(), new FakeDistributedCache());

        context.Expenses.Add(new Expense
        {
            Id = 1,
            Amount = 200,
            Category = "Travel",
            Description = "Cab",
            ExpenseDate = DateTime.UtcNow,
            UserId = 1,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(id: 1, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Category.Should().Be("Travel");
    }

    // ✅ TEST 3: GetByIdAsync - should return null for wrong user (security check)
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotOwnExpense()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance, new FakeMessagePublisher(), new FakeDistributedCache());

        context.Expenses.Add(new Expense
        {
            Id = 1,
            Amount = 200,
            Category = "Travel",
            Description = "Cab",
            ExpenseDate = DateTime.UtcNow,
            UserId = 1,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(id: 1, userId: 99); // wrong user

        // Assert
        result.Should().BeNull();
    }

    // ✅ TEST 4: UpdateAsync - should return true when expense exists
    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenExpenseExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var publisher = new FakeMessagePublisher();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance, publisher, new FakeDistributedCache());

        context.Expenses.Add(new Expense
        {
            Id = 1,
            Amount = 100,
            Category = "Food",
            Description = "Dinner",
            ExpenseDate = DateTime.UtcNow,
            UserId = 1,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var updateRequest = new UpdateExpenseDto
        {
            Amount = 250,
            Category = "Food",
            Description = "Dinner Updated",
            ExpenseDate = DateTime.UtcNow
        };

        // Act
        var result = await service.UpdateAsync(id: 1, updateRequest, userId: 1);

        // Assert
        result.Should().BeTrue();
        publisher.PublishedMessages.Should().ContainSingle();
    }

    // ✅ TEST 5: UpdateAsync - should return false for wrong user
    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserDoesNotOwnExpense()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance, new FakeMessagePublisher(), new FakeDistributedCache());

        context.Expenses.Add(new Expense
        {
            Id = 1,
            Amount = 100,
            Category = "Food",
            Description = "Dinner",
            ExpenseDate = DateTime.UtcNow,
            UserId = 1,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var updateRequest = new UpdateExpenseDto
        {
            Amount = 250,
            Category = "Food",
            Description = "Dinner Updated",
            ExpenseDate = DateTime.UtcNow
        };

        // Act
        var result = await service.UpdateAsync(id: 1, updateRequest, userId: 99); // wrong user

        // Assert
        result.Should().BeFalse();
    }

    // ✅ TEST 6: DeleteAsync - should return true when expense exists
    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenExpenseExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var publisher = new FakeMessagePublisher();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance, publisher, new FakeDistributedCache());

        context.Expenses.Add(new Expense
        {
            Id = 1,
            Amount = 100,
            Category = "Shopping",
            Description = "Shoes",
            ExpenseDate = DateTime.UtcNow,
            UserId = 1,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAsync(id: 1, userId: 1);

        // Assert
        result.Should().BeTrue();
        publisher.PublishedMessages.Should().ContainSingle();
    }

    // ✅ TEST 7: DeleteAsync - should return false for wrong user
    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotOwnExpense()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance, new FakeMessagePublisher(), new FakeDistributedCache());

        context.Expenses.Add(new Expense
        {
            Id = 1,
            Amount = 100,
            Category = "Shopping",
            Description = "Shoes",
            ExpenseDate = DateTime.UtcNow,
            UserId = 1,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAsync(id: 1, userId: 99); // wrong user

        // Assert
        result.Should().BeFalse();
    }
}