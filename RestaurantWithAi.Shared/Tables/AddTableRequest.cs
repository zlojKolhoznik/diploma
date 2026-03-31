using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Tables;

public class AddTableRequest
{
    public int TableNumber { get; set; }

    [Range(0, 100)]
    public int Seats { get; set; }
}
