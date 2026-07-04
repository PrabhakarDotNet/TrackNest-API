using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrackNest.Application.Common;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;
using TrackNest.Application.Events;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;

namespace TrackNest.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly TrackNestDbContext _context;
    private readonly ILogger<ExpenseService> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public ExpenseService(TrackNestDbContext context, ILogger<ExpenseService> logger, IMessagePublisher messagePublisher)
    {
        _context = context;
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    public async Task<PagedResult<ExpenseDto>> GetByUserIdAsync(
        int userId,
        int page = 1,
        int pageSize = 10,
        string sortBy = "expenseDate",
        string sortDirection = "desc",
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId);

        // Apply search BEFORE pagination — filtering after Skip/Take would
        // paginate the full unfiltered set and only filter whatever page
        // happened to load, giving wrong totals and missing matches on
        // page 2+.
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(e =>
                EF.Functions.Like(e.Description, $"%{term}%") ||
                EF.Functions.Like(e.Category, $"%{term}%"));
        }

        query = (sortBy.ToLower(), sortDirection.ToLower()) switch
        {
            ("description", "asc") => query.OrderBy(e => e.Description),
            ("description", "desc") => query.OrderByDescending(e => e.Description),
            ("category", "asc") => query.OrderBy(e => e.Category),
            ("category", "desc") => query.OrderByDescending(e => e.Category),
            ("amount", "asc") => query.OrderBy(e => e.Amount),
            ("amount", "desc") => query.OrderByDescending(e => e.Amount),
            ("expensedate", "asc") => query.OrderBy(e => e.ExpenseDate),
            _ => query.OrderByDescending(e => e.ExpenseDate)
        };

        var totalCount = await query.CountAsync(ct);

        var expenses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Category = e.Category,
                Description = e.Description,
                ExpenseDate = e.ExpenseDate
            })
            .ToListAsync(ct);

        return new PagedResult<ExpenseDto>
        {
            Items = expenses,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<int> CreateAsync(CreateExpenseDto request, int userId, CancellationToken ct = default)
    {
        var expense = new Expense
        {
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
            UserId = userId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created expense {ExpenseId} for user {UserId}", expense.Id, userId);

        await _messagePublisher.PublishAsync(
            new ExpenseChangedEvent { UserId = userId.ToString(), ChangeType = "Created" },
            routingKey: "expense.changed",
            cancellationToken: ct);

        return expense.Id;
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id, int userId, CancellationToken ct = default)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new ExpenseDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Category = x.Category,
                Description = x.Description,
                ExpenseDate = x.ExpenseDate
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateExpenseDto request, int userId, CancellationToken ct = default)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct); // 🔐 SECURITY FIX

        if (expense == null)
            return false;

        expense.Amount = request.Amount;
        expense.Category = request.Category;
        expense.Description = request.Description;
        expense.ExpenseDate = request.ExpenseDate;
        expense.UpdatedBy = userId;
        expense.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated expense {ExpenseId} by user {UserId}", id, userId);

        await _messagePublisher.PublishAsync(
            new ExpenseChangedEvent { UserId = userId.ToString(), ChangeType = "Updated" },
            routingKey: "expense.changed",
            cancellationToken: ct);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct);

        if (expense == null)
            return false;

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted expense {ExpenseId} by user {UserId}", id, userId);

        await _messagePublisher.PublishAsync(
            new ExpenseChangedEvent { UserId = userId.ToString(), ChangeType = "Deleted" },
            routingKey: "expense.changed",
            cancellationToken: ct);

        return true;
    }

}