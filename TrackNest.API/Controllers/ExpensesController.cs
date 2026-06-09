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

        [HttpGet("my-expenses")]
        public async Task<IActionResult> GetMyExpenses()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var expenses = await _expenseService.GetByUserIdAsync(userId);

            return Ok(expenses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var expense = await _expenseService.GetByIdAsync(id);

            if (expense == null)
                return NotFound();

            return Ok(expense);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseDto request)
        {
            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var id = await _expenseService.CreateAsync(request, userId);

            return Ok(new { Id = id });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateExpenseDto request)
        {
            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var updated = await _expenseService.UpdateAsync(id, request, userId);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _expenseService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}