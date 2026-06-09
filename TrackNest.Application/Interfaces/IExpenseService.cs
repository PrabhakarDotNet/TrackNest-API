using TrackNest.Application.DTOs;

namespace TrackNest.Application.Interfaces
{
   public interface IExpenseService
    {

        Task<List<ExpenseDto>> GetAllAsync();

        Task<ExpenseDto?> GetByIdAsync(int id);

        Task<List<ExpenseDto>> GetByUserIdAsync(int userId);

        Task<int> CreateAsync(CreateExpenseDto request, int userId);
        Task<bool> UpdateAsync(int id, UpdateExpenseDto request, int userId);

        Task<bool> DeleteAsync(int id);
    }
}
