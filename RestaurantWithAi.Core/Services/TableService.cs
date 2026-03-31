using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Tables;

namespace RestaurantWithAi.Core.Services;

public class TableService(ITableRepository tableRepository, IMapper mapper) : ITablesService
{
    public async Task<IEnumerable<TableBrief>> GetTablesAsync(Guid restaurantId)
    {
        var tables = await tableRepository.GetTablesByRestaurantIdAsync(restaurantId);
        return mapper.Map<IEnumerable<TableBrief>>(tables);
    }

    public async Task AddTableAsync(Guid restaurantId, AddTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var table = mapper.Map<Table>(request);
        table.RestaurantId = restaurantId;
        await tableRepository.AddTableAsync(table);
    }

    public Task DeleteTableAsync(Guid restaurantId, int tableNumber)
    {
        return tableRepository.DeleteTableAsync(restaurantId, tableNumber);
    }

    public Task UpdateTableSeatsAsync(Guid restaurantId, int tableNumber, UpdateTableSeatsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return tableRepository.UpdateTableSeatsAsync(restaurantId, tableNumber, request.Seats);
    }
}
