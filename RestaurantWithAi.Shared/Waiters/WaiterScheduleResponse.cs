namespace RestaurantWithAi.Shared.Waiters;

public class WaiterScheduleResponse
{
    public Guid Id { get; set; }
    public string WaiterId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly ShiftStart { get; set; }
    public TimeOnly ShiftEnd { get; set; }
}

