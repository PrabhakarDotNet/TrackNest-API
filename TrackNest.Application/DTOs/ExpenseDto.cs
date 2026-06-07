namespace TrackNest.Application.DTOs
{
    public class ExpenseDto
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime ExpenseDate { get; set; }
    }
}
