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
            .ForMember(destination => destination.GuestUserId, options => options.Ignore())
            .ForMember(destination => destination.AssignedWaiterId, options => options.Ignore())
            .ForMember(destination => destination.Status, options => options.Ignore())
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Table, options => options.Ignore())
            .ForMember(destination => destination.AssignedWaiter, options => options.Ignore());
    }
}
