using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackNest.Infrastructure.Services
{
    class ExpenseChangedEvent
    {
        public string UserId { get; set; } = default!;
        public string ChangeType { get; set; } = default!;
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    }
}
