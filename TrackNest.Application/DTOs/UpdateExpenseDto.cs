using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackNest.Application.DTOs
{
    public class UpdateExpenseDto
    {
        public decimal Amount { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime ExpenseDate { get; set; }
    }
}
