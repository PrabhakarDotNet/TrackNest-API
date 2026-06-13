using Microsoft.EntityFrameworkCore;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;

namespace TrackNest.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly TrackNestDbContext _context;

    public ExpenseService(TrackNestDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExpenseDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Select(x => new ExpenseDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Category = x.Category,
                Description = x.Description,
                ExpenseDate = x.ExpenseDate
            })
            .ToListAsync(ct);
    }

    public async Task<List<ExpenseDto>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Category = e.Category,
                Description = e.Description,
                ExpenseDate = e.ExpenseDate
            })
            .ToListAsync(ct);
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

        return expense.Id;
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id, int userId, CancellationToken ct = default)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)   // 🔐 SECURITY FIX
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

        return true;
    }
}