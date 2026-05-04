using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly RestaurantDbContext dbContext;

    public ReviewRepository(RestaurantDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddReviewAsync(Review review)
    {
        await dbContext.Reviews.AddAsync(review);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Review?> GetByReservationIdAsync(Guid reservationId)
    {
        return await dbContext.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.ReservationId == reservationId);
    }

    public async Task<IEnumerable<Review>> GetReviewsForRestaurantAsync(Guid restaurantId)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Reservation)
            .Where(r => r.Reservation.RestaurantId == restaurantId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync();
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }

    public async Task<decimal?> GetAverageRestaurantRatingAsync(Guid restaurantId)
    {
        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Reservation)
            .Where(r => r.Reservation.RestaurantId == restaurantId)
            .ToListAsync();

        if (reviews.Count == 0)
            return null;

        // Average of (cuisine + service) / 2 for all reviews
        var averageRating = reviews
            .Select(r => ((decimal)r.CuisineRating + r.ServiceRating) / 2)
            .Average();

        return Math.Round(averageRating, 1);
    }

    public async Task<decimal?> GetAverageWaiterRatingAsync(string waiterId)
    {
        if (string.IsNullOrWhiteSpace(waiterId))
            return null;

        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Reservation)
            .Where(r => r.Reservation.AssignedWaiterId == waiterId)
            .ToListAsync();

        if (reviews.Count == 0)
            return null;

        // Average of (cuisine + service) / 2 for all reviews
        var averageRating = reviews
            .Select(r => ((decimal)r.CuisineRating + r.ServiceRating) / 2)
            .Average();

        return Math.Round(averageRating, 1);
    }
}
