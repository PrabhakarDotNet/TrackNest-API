using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TrackNest.Application.DTOs;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;
using TrackNest.Infrastructure.Services;

namespace TrackNest.Tests;

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
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance);

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
    }

    // ✅ TEST 2: GetByIdAsync - should return expense for correct user
    [Fact]
    public async Task GetByIdAsync_ShouldReturnExpense_WhenUserOwnsIt()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance);

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
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance);

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
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance);

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
    }

    // ✅ TEST 5: UpdateAsync - should return false for wrong user
    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserDoesNotOwnExpense()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance);

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
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance);

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
    }

    // ✅ TEST 7: DeleteAsync - should return false for wrong user
    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotOwnExpense()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ExpenseService(context, NullLogger<ExpenseService>.Instance);

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