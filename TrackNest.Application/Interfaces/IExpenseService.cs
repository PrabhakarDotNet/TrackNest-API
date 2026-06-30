using TrackNest.Application.Common;
using TrackNest.Application.DTOs;

namespace TrackNest.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<PagedResult<ExpenseDto>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10, string sortBy = "expenseDate", string sortDirection = "desc", CancellationToken ct = default);
        Task<int> CreateAsync(CreateExpenseDto request, int userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id, UpdateExpenseDto request, int userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<ExpenseDto?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
    }
}
