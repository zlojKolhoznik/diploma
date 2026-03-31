using Amazon.CognitoIdentityProvider.Model;
using AutoMapper;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Core.Mappings;

public class WaiterMappingProfile : Profile
{
    private const string RestaurantIdAttributeName = "custom:restaurantId";

    public WaiterMappingProfile()
    {
        CreateMap<UserType, WaiterResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => GetAttributeValue(src.Attributes, "email") ?? string.Empty))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => GetAttributeValue(src.Attributes, "given_name")))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => GetAttributeValue(src.Attributes, "family_name")))
            .ForMember(dest => dest.RestaurantId, opt => opt.MapFrom(src => GetAttributeValue(src.Attributes, RestaurantIdAttributeName)));
    }

    private static string? GetAttributeValue(List<AttributeType> attributes, string name)
    {
        return attributes?.FirstOrDefault(a => a.Name == name)?.Value;
    }
}
