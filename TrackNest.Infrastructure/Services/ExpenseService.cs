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
    public async Task<List<ExpenseDto>> GetAllAsync()
    {
        return await _context.Expenses
            .Select(x => new ExpenseDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Category = x.Category,
                Description = x.Description,
                ExpenseDate = x.ExpenseDate
            })
            .ToListAsync();
    }
    public async Task<int> CreateAsync(CreateExpenseDto request)
    {
        var expense = new Expense
        {
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
            UserId = 1000,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = 1000
        };

        _context.Expenses.Add(expense);

        await _context.SaveChangesAsync();

        return expense.Id;
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        return await _context.Expenses
            .Where(x => x.Id == id)
            .Select(x => new ExpenseDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Category = x.Category,
                Description = x.Description,
                ExpenseDate = x.ExpenseDate
            })
            .FirstOrDefaultAsync();
    }
    public async Task<bool> UpdateAsync(int id, UpdateExpenseDto request)
    {
        var expense = await _context.Expenses.FindAsync(id);

        if (expense == null)
            return false;

        expense.Amount = request.Amount;
        expense.Category = request.Category;
        expense.Description = request.Description;
        expense.ExpenseDate = request.ExpenseDate;
        expense.UpdatedBy = 1000;
        expense.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);

        if (expense == null)
            return false;

        _context.Expenses.Remove(expense);

        await _context.SaveChangesAsync();

        return true;
    }
}