namespace TrackNest.Application.Events;

public class ExpenseChangedEvent
{
    public string UserId { get; set; } = default!;
    public string ChangeType { get; set; } = default!;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}