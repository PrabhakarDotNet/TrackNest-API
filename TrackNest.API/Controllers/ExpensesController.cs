using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;

namespace TrackNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException();

            return userId;
        }

        [HttpGet("my-expenses")]
        public async Task<IActionResult> GetMyExpenses(CancellationToken ct)
        {
            var userId = GetUserId();

            var expenses = await _expenseService.GetByUserIdAsync(userId, ct);

            return Ok(expenses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var userId = GetUserId();

            var expense = await _expenseService.GetByIdAsync(id, userId, ct);

            if (expense == null)
                return NotFound();

            return Ok(expense);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseDto request, CancellationToken ct)
        {
            var userId = GetUserId();

            var id = await _expenseService.CreateAsync(request, userId, ct);

            return Ok(new { Id = id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateExpenseDto request, CancellationToken ct)
        {
            var userId = GetUserId();

            var updated = await _expenseService.UpdateAsync(id, request, userId, ct);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var userId = GetUserId();

            var deleted = await _expenseService.DeleteAsync(id, userId, ct);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}