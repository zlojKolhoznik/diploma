using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Tables;

namespace RestaurantWithAi.Core.Mappings;

public class TableMappingProfile : Profile
{
    public TableMappingProfile()
    {
        CreateMap<Table, TableBrief>();
        CreateMap<AddTableRequest, Table>()
            .ForMember(destination => destination.RestaurantId, options => options.Ignore())
            .ForMember(destination => destination.Restaurant, options => options.Ignore());
    }
}
