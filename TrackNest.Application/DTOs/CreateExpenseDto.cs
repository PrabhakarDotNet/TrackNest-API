namespace TrackNest.Application.DTOs
{
    public class CreateExpenseDto
    {
        public decimal Amount { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime ExpenseDate { get; set; }
       
    }
}
