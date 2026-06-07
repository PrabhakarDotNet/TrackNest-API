using TrackNest.Application.DTOs;

namespace TrackNest.Application.Interfaces
{
   public interface IExpenseService
    {
        Task<int> CreateAsync(CreateExpenseDto request);

        Task<List<ExpenseDto>> GetAllAsync();

        Task<ExpenseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UpdateExpenseDto request);

        Task<bool> DeleteAsync(int id);
    }
}
