using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Restaurants;

public class CreateRestaurantRequest
{
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;
}

