namespace RestaurantWithAi.Shared.Dishes;

public class DishBrief
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = null!;
}