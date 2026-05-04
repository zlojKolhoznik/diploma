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
}

