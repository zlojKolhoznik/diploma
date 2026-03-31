namespace RestaurantWithAi.Shared.Tables;

public interface ITablesService
{
    Task<IEnumerable<TableBrief>> GetTablesAsync(Guid restaurantId);
    Task AddTableAsync(Guid restaurantId, AddTableRequest request);
    Task DeleteTableAsync(Guid restaurantId, int tableNumber);
    Task UpdateTableSeatsAsync(Guid restaurantId, int tableNumber, UpdateTableSeatsRequest request);
}
