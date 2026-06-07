using Microsoft.AspNetCore.Mvc;
using TrackNest.Application.DTOs;
using TrackNest.Application.Interfaces;
using TrackNest.Domain.Entities;
using TrackNest.Infrastructure.Persistence;

namespace TrackNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var expense = await _expenseService.GetByIdAsync(id);

            if (expense == null)
                return NotFound();

            return Ok(expense);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var expenses = await _expenseService.GetAllAsync();

            return Ok(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseDto request)
        {
            var id = await _expenseService.CreateAsync(request);

            return Ok(new { Id = id });

        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateExpenseDto request)
        {
            var updated = await _expenseService.UpdateAsync(id, request);

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
