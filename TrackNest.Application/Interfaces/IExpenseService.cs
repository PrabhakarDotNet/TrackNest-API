using TrackNest.Application.DTOs;

namespace TrackNest.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<List<ExpenseDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<ExpenseDto?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);

        Task<List<ExpenseDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

        Task<int> CreateAsync(CreateExpenseDto request, int userId, CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(int id, UpdateExpenseDto request, int userId, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
    }
}