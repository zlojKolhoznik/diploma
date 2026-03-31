using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Data.Models;

public class Waiter
{
    public int Id { get; set; }

    [Required]
    public required string UserId { get; set; }

    public string? RestaurantId { get; set; }
}
