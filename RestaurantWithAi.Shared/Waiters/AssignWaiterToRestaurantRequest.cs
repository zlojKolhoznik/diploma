namespace RestaurantWithAi.Shared.Waiters;

public class AssignWaiterToRestaurantRequest
{
    public required string UserId { get; set; }
    public required string RestaurantId { get; set; }
}
