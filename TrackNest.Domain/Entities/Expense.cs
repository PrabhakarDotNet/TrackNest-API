namespace TrackNest.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime ExpenseDate { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedOn { get; set; }

        public long CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long UpdatedBy { get; set; }
    }
}
