using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Orders;

namespace RestaurantWithAi.Core.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderItem, OrderItemResponse>();

        CreateMap<Order, OrderResponse>();

        CreateMap<CreateOrderRequest, Order>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.RestaurantId, options => options.Ignore())
            .ForMember(destination => destination.ReservationId, options => options.Ignore())
            .ForMember(destination => destination.Status, options => options.Ignore())
            .ForMember(destination => destination.CreatedAtUtc, options => options.Ignore())
            .ForMember(destination => destination.ClosedAtUtc, options => options.Ignore())
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Reservation, options => options.Ignore())
            .ForMember(destination => destination.Items, options => options.Ignore());
    }
}

