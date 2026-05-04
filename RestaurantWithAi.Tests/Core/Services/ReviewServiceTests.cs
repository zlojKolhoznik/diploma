using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Moq;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReviewServiceTests
{
    [Fact]
    public async Task CreateReviewAsync_WhenNoExistingReview_AddsReview()
    {
        var reviewRepo = new Mock<IReviewRepository>();
        var reservationRepo = new Mock<IReservationRepository>();
        var restaurantRepo = new Mock<IRestaurantRepository>();
        var waiterRepo = new Mock<IWaiterRepository>();

        var reservationId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        Review? captured = null;

        reservationRepo.Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(new Reservation { Id = reservationId, RestaurantId = restaurantId, StartTime = DateTime.UtcNow, ApproximateDurationMinutes = 60, NumberOfGuests = 2, Status = ReservationStatuses.Created });

        reviewRepo.Setup(r => r.GetByReservationIdAsync(reservationId)).ReturnsAsync((Review?)null);
        reviewRepo.Setup(r => r.AddReviewAsync(It.IsAny<Review>()))
            .Callback<Review>(r => captured = r)
            .Returns(Task.CompletedTask);
        reviewRepo.Setup(r => r.GetAverageRestaurantRatingAsync(restaurantId)).ReturnsAsync(4.5m);
        reviewRepo.Setup(r => r.GetAverageWaiterRatingAsync(It.IsAny<string>())).ReturnsAsync((decimal?)null);

        restaurantRepo.Setup(r => r.GetRestaurantByIdAsync(restaurantId, null, null))
            .ReturnsAsync(new Restaurant { Id = restaurantId, City = "Test", Address = "Test" });
        restaurantRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ReviewMappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        var sut = new ReviewService(reviewRepo.Object, reservationRepo.Object, restaurantRepo.Object, waiterRepo.Object, mapper);

        await sut.CreateReviewAsync(reservationId, new CreateReviewRequest { CuisineRating = 5, ServiceRating = 4 }, "user-1", isAdmin: false);

        Assert.NotNull(captured);
        Assert.Equal(reservationId, captured!.ReservationId);
        Assert.Equal(5, captured.CuisineRating);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenReviewExists_ThrowsInvalidOperationException()
    {
        var reviewRepo = new Mock<IReviewRepository>();
        var reservationRepo = new Mock<IReservationRepository>();
        var restaurantRepo = new Mock<IRestaurantRepository>();
        var waiterRepo = new Mock<IWaiterRepository>();

        var reservationId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        reservationRepo.Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(new Reservation { Id = reservationId, RestaurantId = restaurantId, StartTime = DateTime.UtcNow, ApproximateDurationMinutes = 60, NumberOfGuests = 2, Status = ReservationStatuses.Created });

        reviewRepo.Setup(r => r.GetByReservationIdAsync(reservationId)).ReturnsAsync(new Review { Id = Guid.NewGuid(), ReservationId = reservationId, CuisineRating = 4, ServiceRating = 4 });

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ReviewMappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        var sut = new ReviewService(reviewRepo.Object, reservationRepo.Object, restaurantRepo.Object, waiterRepo.Object, mapper);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateReviewAsync(reservationId, new CreateReviewRequest { CuisineRating = 5, ServiceRating = 5 }, "user-1", isAdmin: false));
    }
}


