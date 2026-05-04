using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Core.Mappings;

public class WaiterScheduleMappingProfile : Profile
{
    public WaiterScheduleMappingProfile()
    {
        CreateMap<WaiterSchedule, WaiterScheduleResponse>();
        CreateMap<CreateWaiterScheduleRequest, WaiterSchedule>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Waiter, options => options.Ignore());
    }
}

