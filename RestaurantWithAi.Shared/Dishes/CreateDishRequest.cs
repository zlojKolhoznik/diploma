namespace RestaurantWithAi.Shared.Dishes;

public class CreateDishRequest
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public string ImageUrl { get; set; } = string.Empty;
}