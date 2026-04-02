using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface ITableRepository
{
    Task<IEnumerable<Table>> GetTablesByRestaurantIdAsync(Guid restaurantId);
    Task<IEnumerable<Table>> GetAvailableTablesAsync(Guid restaurantId, DateTimeOffset startTime, int durationMinutes);
    Task AddTableAsync(Table table);
    Task DeleteTableAsync(Guid restaurantId, int tableNumber);
    Task UpdateTableSeatsAsync(Guid restaurantId, int tableNumber, int seats);
}
