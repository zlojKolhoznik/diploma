using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Core.Mappings;

public class RestaurantMappingProfile : Profile
{
    public RestaurantMappingProfile()
    {
        CreateMap<Restaurant, RestaurantBrief>();
        CreateMap<Restaurant, RestaurantDetail>();
        CreateMap<CreateRestaurantRequest, Restaurant>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.AvailableDishes, options => options.Ignore())
            .ForMember(destination => destination.Tables, options => options.Ignore());
    }
}

