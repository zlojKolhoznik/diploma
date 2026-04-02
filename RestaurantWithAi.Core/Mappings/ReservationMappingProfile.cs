using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Core.Mappings;

public class ReservationMappingProfile : Profile
{
    public ReservationMappingProfile()
    {
        CreateMap<Reservation, ReservationResponse>();
        CreateMap<CreateReservationRequest, Reservation>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Status, options => options.Ignore())
            .ForMember(destination => destination.TableNumber, options => options.Ignore())
            .ForMember(destination => destination.WaiterId, options => options.Ignore())
            .ForMember(destination => destination.Restaurant, options => options.Ignore());
    }
}
