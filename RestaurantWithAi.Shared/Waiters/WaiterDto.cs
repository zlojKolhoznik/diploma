namespace RestaurantWithAi.Shared.Waiters;

public class WaiterDto
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public string? RestaurantId { get; set; }
}
