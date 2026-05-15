namespace RestaurantWithAi.Core.Contracts;

// ── Analytics data models ─────────────────────────────────────────────────────

public class RestaurantProfitabilityData
{
    public Guid RestaurantId { get; set; }
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalReservations { get; set; }
    public decimal? AverageRating { get; set; }
}

public class DishOrderData
{
    public Guid DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int TotalQuantityOrdered { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageUnitPrice { get; set; }
}

public class WaiterPerformanceData
{
    public string WaiterId { get; set; } = string.Empty;
    public int TotalReservations { get; set; }
    public decimal? AverageCuisineRating { get; set; }
    public decimal? AverageServiceRating { get; set; }
    public IReadOnlyList<string> RecentComments { get; set; } = [];
}

// ── Repository contract ───────────────────────────────────────────────────────

public interface IReportingDataRepository
{
    /// <summary>
    /// Returns revenue, order count, reservation count and average rating
    /// per restaurant. Pass <paramref name="restaurantId"/> to scope to one location.
    /// </summary>
    Task<IReadOnlyList<RestaurantProfitabilityData>> GetProfitabilityAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? restaurantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns order quantity and revenue aggregated per dish (approved items only).
    /// </summary>
    Task<IReadOnlyList<DishOrderData>> GetDishOrderStatsAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? restaurantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns per-waiter reservation count and average cuisine/service ratings
    /// derived from reviews on their reservations.
    /// </summary>
    Task<IReadOnlyList<WaiterPerformanceData>> GetWaiterPerformanceAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? restaurantId,
        CancellationToken cancellationToken = default);
}

