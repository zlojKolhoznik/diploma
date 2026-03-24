using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Core.Entities;

public class Restaurant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public required string City { get; set; }
    
    [Required]
    public required string Address { get; set; }
    
    public ICollection<Dish> AvailableDishes { get; set; } = new List<Dish>();
}