namespace RestaurantWithAi.Data.Entities;

public class Waiter
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int? RestaurantId { get; set; }
}
