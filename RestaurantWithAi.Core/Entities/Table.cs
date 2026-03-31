using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class Table
{
    public int TableNumber { get; set; }

    public int Seats { get; set; }

    [ForeignKey(nameof(Restaurant))]
    public Guid RestaurantId { get; set; }

    public Restaurant Restaurant { get; set; } = null!;
}
