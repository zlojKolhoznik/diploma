using RestaurantWithAi.Shared.Dishes;

namespace RestaurantWithAi.Shared.Restaurants;

public class RestaurantDetail
{
    public Guid Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public IEnumerable<DishBrief> AvailableDishes { get; set; } = [];
    public bool? HasAvailablePlaces { get; set; }
}

