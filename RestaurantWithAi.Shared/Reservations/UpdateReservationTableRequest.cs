using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationTableRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "TableNumber must be at least 1.")]
    public int TableNumber { get; set; }
}
