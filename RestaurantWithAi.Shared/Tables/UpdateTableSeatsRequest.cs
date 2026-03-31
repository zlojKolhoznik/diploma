using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Tables;

public class UpdateTableSeatsRequest
{
    [Range(0, 100)]
    public int Seats { get; set; }
}
