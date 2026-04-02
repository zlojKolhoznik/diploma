namespace RestaurantWithAi.Shared.Restaurants;

public class RestaurantBrief
{
    public Guid Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool? HasAvailablePlaces { get; set; }
}

