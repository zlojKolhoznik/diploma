using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Moq;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Tables;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class TableServiceTests
{
    #region GetTablesAsync

    [Fact]
    public async Task GetTablesAsync_WhenRepositoryReturnsTables_ReturnsMappedTableBriefCollection()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();
        var tables = new List<Table>
        {
            CreateTable(1, 4, restaurantId),
            CreateTable(2, 2, restaurantId)
        };

        repositoryMock
            .Setup(r => r.GetTablesByRestaurantIdAsync(restaurantId))
            .ReturnsAsync(tables);

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetTablesAsync(restaurantId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.TableNumber == 1 && t.Seats == 4);
        Assert.Contains(result, t => t.TableNumber == 2 && t.Seats == 2);
        repositoryMock.Verify(r => r.GetTablesByRestaurantIdAsync(restaurantId), Times.Once);
    }

    [Fact]
    public async Task GetTablesAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyCollection()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetTablesByRestaurantIdAsync(restaurantId))
            .ReturnsAsync([]);

        var sut = CreateSut(repositoryMock.Object);

        var result = await sut.GetTablesAsync(restaurantId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTablesAsync_WhenRestaurantNotFound_PropagatesKeyNotFoundException()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetTablesByRestaurantIdAsync(restaurantId))
            .ThrowsAsync(new KeyNotFoundException($"Restaurant with ID {restaurantId} not found"));

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetTablesAsync(restaurantId));
    }

    #endregion

    #region AddTableAsync

    [Fact]
    public async Task AddTableAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<ITableRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddTableAsync(Guid.NewGuid(), null!));
        repositoryMock.Verify(r => r.AddTableAsync(It.IsAny<Table>()), Times.Never);
    }

    [Fact]
    public async Task AddTableAsync_WhenRequestIsValid_MapsAndDelegatesToRepository()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();
        Table? capturedTable = null;

        repositoryMock
            .Setup(r => r.AddTableAsync(It.IsAny<Table>()))
            .Callback<Table>(t => capturedTable = t)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var request = new AddTableRequest { TableNumber = 3, Seats = 4 };

        await sut.AddTableAsync(restaurantId, request);

        Assert.NotNull(capturedTable);
        Assert.Equal(3, capturedTable!.TableNumber);
        Assert.Equal(4, capturedTable.Seats);
        Assert.Equal(restaurantId, capturedTable.RestaurantId);
        repositoryMock.Verify(r => r.AddTableAsync(It.IsAny<Table>()), Times.Once);
    }

    [Fact]
    public async Task AddTableAsync_WhenRestaurantNotFound_PropagatesKeyNotFoundException()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.AddTableAsync(It.IsAny<Table>()))
            .ThrowsAsync(new KeyNotFoundException($"Restaurant with ID {restaurantId} not found"));

        var sut = CreateSut(repositoryMock.Object);
        var request = new AddTableRequest { TableNumber = 1, Seats = 4 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.AddTableAsync(restaurantId, request));
    }

    #endregion

    #region DeleteTableAsync

    [Fact]
    public async Task DeleteTableAsync_DelegatesToRepository()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.DeleteTableAsync(restaurantId, 1))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.DeleteTableAsync(restaurantId, 1);

        repositoryMock.Verify(r => r.DeleteTableAsync(restaurantId, 1), Times.Once);
    }

    [Fact]
    public async Task DeleteTableAsync_WhenTableNotFound_PropagatesKeyNotFoundException()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.DeleteTableAsync(restaurantId, 99))
            .ThrowsAsync(new KeyNotFoundException($"Table 99 for restaurant {restaurantId} not found"));

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.DeleteTableAsync(restaurantId, 99));
    }

    #endregion

    #region UpdateTableSeatsAsync

    [Fact]
    public async Task UpdateTableSeatsAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<ITableRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateTableSeatsAsync(Guid.NewGuid(), 1, null!));
        repositoryMock.Verify(r => r.UpdateTableSeatsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTableSeatsAsync_WhenRequestIsValid_DelegatesToRepository()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.UpdateTableSeatsAsync(restaurantId, 2, 6))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateTableSeatsRequest { Seats = 6 };

        await sut.UpdateTableSeatsAsync(restaurantId, 2, request);

        repositoryMock.Verify(r => r.UpdateTableSeatsAsync(restaurantId, 2, 6), Times.Once);
    }

    [Fact]
    public async Task UpdateTableSeatsAsync_WhenTableNotFound_PropagatesKeyNotFoundException()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.UpdateTableSeatsAsync(restaurantId, 99, It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException($"Table 99 for restaurant {restaurantId} not found"));

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateTableSeatsRequest { Seats = 4 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateTableSeatsAsync(restaurantId, 99, request));
    }

    #endregion

    #region GetAvailableTablesAsync

    [Fact]
    public async Task GetAvailableTablesAsync_WhenRepositoryReturnsTables_ReturnsMappedCollection()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);
        var tables = new List<Table>
        {
            CreateTable(1, 4, restaurantId),
            CreateTable(2, 2, restaurantId)
        };

        repositoryMock
            .Setup(r => r.GetAvailableTablesAsync(restaurantId, startTime, 60))
            .ReturnsAsync(tables);

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetAvailableTablesAsync(restaurantId, startTime, 60)).ToList();

        Assert.Equal(2, result.Count);
        repositoryMock.Verify(r => r.GetAvailableTablesAsync(restaurantId, startTime, 60), Times.Once);
    }

    [Fact]
    public async Task GetAvailableTablesAsync_WhenRestaurantNotFound_PropagatesKeyNotFoundException()
    {
        var repositoryMock = new Mock<ITableRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetAvailableTablesAsync(restaurantId, It.IsAny<DateTime>(), It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException($"Restaurant with ID {restaurantId} not found"));

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetAvailableTablesAsync(restaurantId, DateTime.UtcNow, 60));
    }

    #endregion

    private static TableService CreateSut(ITableRepository repository)
    {
        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<TableMappingProfile>());
        var mapper = mapperConfiguration.CreateMapper();
        return new TableService(repository, mapper);
    }

    private static Table CreateTable(int tableNumber, int seats, Guid restaurantId)
    {
        return new Table
        {
            TableNumber = tableNumber,
            Seats = seats,
            RestaurantId = restaurantId
        };
    }
}
