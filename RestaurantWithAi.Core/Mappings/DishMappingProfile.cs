using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Dishes;

namespace RestaurantWithAi.Core.Mappings;

public class DishMappingProfile : Profile
{
    public DishMappingProfile()
    {
        CreateMap<Dish, DishBrief>().ReverseMap();
        CreateMap<Dish, DishDetail>().ReverseMap();
        CreateMap<CreateDishRequest, Dish>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.AvailableAtRestaurants, options => options.Ignore())
            .ForMember(destination => destination.OrderItems, options => options.Ignore());
        CreateMap<Dish, CreateDishRequest>();
    }
}
