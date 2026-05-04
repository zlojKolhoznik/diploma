using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Core.Mappings;

public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        CreateMap<CreateReviewRequest, Review>()
            .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Reservation, opt => opt.Ignore());

        CreateMap<Review, ReviewResponse>();
    }
}

