namespace RestaurantWithAi.Shared.Waiters;

public class WaiterResponse
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RestaurantId { get; set; }
}
