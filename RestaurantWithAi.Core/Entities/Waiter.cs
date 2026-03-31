using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class Waiter
{
    [Key]
    public required string UserId { get; set; }

    [ForeignKey(nameof(Restaurant))]
    public Guid? RestaurantId { get; set; }

    public Restaurant? Restaurant { get; set; }
}
