using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class ReportingDataRepository(RestaurantDbContext db) : IReportingDataRepository
{
    public async Task<IReadOnlyList<RestaurantProfitabilityData>> GetProfitabilityAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? restaurantId,
        CancellationToken cancellationToken = default)
    {
        // Base order query scoped by date range and optional restaurant
        var ordersQuery = db.Orders
            .AsNoTracking()
            .Where(o => (fromUtc == null || o.CreatedAtUtc >= fromUtc)
                     && (toUtc   == null || o.CreatedAtUtc <= toUtc)
                     && (restaurantId == null || o.RestaurantId == restaurantId));

        // Revenue = sum of approved item lines (UnitPrice * Quantity)
        var revenueByRestaurant = await db.OrderItems
            .AsNoTracking()
            .Where(i => i.Status != OrderItemStatuses.Rejected)
            .Join(ordersQuery, i => i.OrderId, o => o.Id,
                (i, o) => new { o.RestaurantId, Revenue = i.UnitPrice * i.Quantity })
            .GroupBy(x => x.RestaurantId)
            .Select(g => new { RestaurantId = g.Key, TotalRevenue = g.Sum(x => x.Revenue) })
            .ToListAsync(cancellationToken);

        var orderCountByRestaurant = await ordersQuery
            .GroupBy(o => o.RestaurantId)
            .Select(g => new { RestaurantId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var reservationCountByRestaurant = await db.Reservations
            .AsNoTracking()
            .Where(r => (fromUtc == null || r.StartTime >= fromUtc)
                     && (toUtc   == null || r.StartTime <= toUtc)
                     && (restaurantId == null || r.RestaurantId == restaurantId))
            .GroupBy(r => r.RestaurantId)
            .Select(g => new { RestaurantId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Collect all restaurant IDs that appear in any of the above
        var allRestaurantIds = revenueByRestaurant.Select(x => x.RestaurantId)
            .Union(orderCountByRestaurant.Select(x => x.RestaurantId))
            .Union(reservationCountByRestaurant.Select(x => x.RestaurantId))
            .Distinct()
            .ToList();

        if (restaurantId.HasValue && !allRestaurantIds.Contains(restaurantId.Value))
            allRestaurantIds.Add(restaurantId.Value);

        var restaurants = await db.Restaurants
            .AsNoTracking()
            .Where(r => allRestaurantIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        var revMap   = revenueByRestaurant.ToDictionary(x => x.RestaurantId, x => x.TotalRevenue);
        var ordMap   = orderCountByRestaurant.ToDictionary(x => x.RestaurantId, x => x.Count);
        var resMap   = reservationCountByRestaurant.ToDictionary(x => x.RestaurantId, x => x.Count);

        return restaurants
            .Select(r => new RestaurantProfitabilityData
            {
                RestaurantId      = r.Id,
                City              = r.City,
                Address           = r.Address,
                TotalRevenue      = revMap.GetValueOrDefault(r.Id, 0m),
                TotalOrders       = ordMap.GetValueOrDefault(r.Id, 0),
                TotalReservations = resMap.GetValueOrDefault(r.Id, 0),
                AverageRating     = r.AverageRating
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();
    }

    public async Task<IReadOnlyList<DishOrderData>> GetDishOrderStatsAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? restaurantId,
        CancellationToken cancellationToken = default)
    {
        var ordersQuery = db.Orders
            .AsNoTracking()
            .Where(o => (fromUtc == null || o.CreatedAtUtc >= fromUtc)
                     && (toUtc   == null || o.CreatedAtUtc <= toUtc)
                     && (restaurantId == null || o.RestaurantId == restaurantId));

        var stats = await db.OrderItems
            .AsNoTracking()
            .Where(i => i.Status != OrderItemStatuses.Rejected)
            .Join(ordersQuery, i => i.OrderId, o => o.Id, (i, _) => i)
            .GroupBy(i => new { i.DishId, i.DishName })
            .Select(g => new DishOrderData
            {
                DishId               = g.Key.DishId,
                DishName             = g.Key.DishName,
                TotalQuantityOrdered = g.Sum(i => i.Quantity),
                TotalRevenue         = g.Sum(i => i.UnitPrice * i.Quantity),
                AverageUnitPrice     = g.Average(i => i.UnitPrice)
            })
            .OrderByDescending(x => x.TotalQuantityOrdered)
            .ToListAsync(cancellationToken);

        return stats;
    }

    public async Task<IReadOnlyList<WaiterPerformanceData>> GetWaiterPerformanceAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? restaurantId,
        CancellationToken cancellationToken = default)
    {
        var reservationsQuery = db.Reservations
            .AsNoTracking()
            .Where(r => r.AssignedWaiterId != null
                     && (fromUtc == null || r.StartTime >= fromUtc)
                     && (toUtc   == null || r.StartTime <= toUtc)
                     && (restaurantId == null || r.RestaurantId == restaurantId));

        // Reservation counts per waiter
        var reservationCounts = await reservationsQuery
            .GroupBy(r => r.AssignedWaiterId!)
            .Select(g => new { WaiterId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var waiterIds = reservationCounts.Select(x => x.WaiterId).ToList();

        // Fetch reviews linked to these waiters' reservations in the period
        var reviewData = await db.Reviews
            .AsNoTracking()
            .Join(reservationsQuery.Where(r => r.AssignedWaiterId != null),
                rev => rev.ReservationId, res => res.Id,
                (rev, res) => new
                {
                    WaiterId       = res.AssignedWaiterId!,
                    rev.CuisineRating,
                    rev.ServiceRating,
                    rev.CuisineComment,
                    rev.ServiceComment
                })
            .ToListAsync(cancellationToken);

        var reviewsByWaiter = reviewData
            .GroupBy(x => x.WaiterId)
            .ToDictionary(
                g => g.Key,
                g => g.ToList());

        var countMap = reservationCounts.ToDictionary(x => x.WaiterId, x => x.Count);

        return waiterIds
            .Select(wid =>
            {
                var reviews = reviewsByWaiter.GetValueOrDefault(wid, []);
                var comments = reviews
                    .SelectMany(r => new[]
                    {
                        string.IsNullOrWhiteSpace(r.CuisineComment) ? null : $"Cuisine: {r.CuisineComment}",
                        string.IsNullOrWhiteSpace(r.ServiceComment) ? null : $"Service: {r.ServiceComment}"
                    })
                    .Where(c => c != null)
                    .Take(5)
                    .ToList()!;

                return new WaiterPerformanceData
                {
                    WaiterId             = wid,
                    TotalReservations    = countMap.GetValueOrDefault(wid, 0),
                    AverageCuisineRating = reviews.Count > 0 ? (decimal?)reviews.Average(r => r.CuisineRating) : null,
                    AverageServiceRating = reviews.Count > 0 ? (decimal?)reviews.Average(r => r.ServiceRating) : null,
                    RecentComments       = comments!
                };
            })
            .OrderByDescending(x => x.TotalReservations)
            .ToList();
    }
}

