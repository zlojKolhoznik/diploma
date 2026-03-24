using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Dishes;

public class CreateDishRequest
{
	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(500)]
	public string Description { get; set; } = string.Empty;

	[Range(0.01, 100000)]
	public decimal Price { get; set; }

	[Required]
	[MaxLength(2048)]
	[Url]
	public string ImageUrl { get; set; } = string.Empty;
}