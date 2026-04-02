using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationTableRequest
{
    [Range(1, int.MaxValue)]
    public int TableNumber { get; set; }
}

