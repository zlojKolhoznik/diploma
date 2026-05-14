using AutoMapper;
using Microsoft.Extensions.Logging;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Core.Services;

public class RestaurantService : IRestaurantsService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IMapper _mapper;
    private readonly IImageStorageService? _imageStorage;
    private readonly ILogger<RestaurantService>? _logger;

    public RestaurantService(IRestaurantRepository restaurantRepository, ITableRepository tableRepository, IMapper mapper)
        : this(restaurantRepository, tableRepository, mapper, null, null)
    {
    }

    public RestaurantService(IRestaurantRepository restaurantRepository, ITableRepository tableRepository, IMapper mapper, IImageStorageService? imageStorage, ILogger<RestaurantService>? logger)
    {
        _restaurantRepository = restaurantRepository;
        _tableRepository = tableRepository;
        _mapper = mapper;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<IEnumerable<RestaurantBrief>> GetRestaurantsAsync(string? city = null, DateTime? time = null, int? durationMinutes = null)
    {
        ValidateAvailabilityArguments(time, durationMinutes);

        var restaurants = (await _restaurantRepository.GetAllRestaurantsAsync(city, time, durationMinutes)).ToList();
        var result = _mapper.Map<List<RestaurantBrief>>(restaurants);

        if (time.HasValue && durationMinutes.HasValue)
        {
            for (var index = 0; index < restaurants.Count; index++)
            {
                result[index].HasAvailablePlaces = await _tableRepository.HasAvailableTablesAsync(
                    restaurants[index].Id,
                    EnsureUtc(time.Value),
                    durationMinutes.Value);
            }
        }

        return result;
    }

    public async Task<RestaurantDetail> GetRestaurantDetailAsync(Guid id, DateTime? time = null, int? durationMinutes = null)
    {
        ValidateAvailabilityArguments(time, durationMinutes);

        var restaurant = await _restaurantRepository.GetRestaurantByIdAsync(id, time, durationMinutes);
        var result = _mapper.Map<RestaurantDetail>(restaurant);

        if (time.HasValue && durationMinutes.HasValue)
            {
            result.HasAvailablePlaces = await _tableRepository.HasAvailableTablesAsync(
                id,
                EnsureUtc(time.Value),
                durationMinutes.Value);
        }

        return result;
    }

    public async Task CreateRestaurantAsync(CreateRestaurantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var restaurant = _mapper.Map<Restaurant>(request);
        await _restaurantRepository.AddRestaurantAsync(restaurant);
    }

    public async Task UpdateRestaurantAsync(Guid id, CreateRestaurantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var restaurant = _mapper.Map<Restaurant>(request);
        restaurant.Id = id;
        await _restaurantRepository.UpdateRestaurantAsync(restaurant);
    }

    public Task DeleteRestaurantAsync(Guid id)
    {
        return _restaurantRepository.DeleteRestaurantAsync(id);
    }

    public async Task<string> UploadRestaurantImageAsync(Guid id, System.IO.Stream content, string fileName, string contentType)
    {
        if (_imageStorage is null)
            throw new InvalidOperationException("Image storage is not configured.");

        var restaurant = await _restaurantRepository.GetRestaurantByIdAsync(id);

        if (!string.IsNullOrWhiteSpace(restaurant.ImageUrl))
        {
            var oldKey = ExtractKeyFromS3Url(restaurant.ImageUrl);
            if (!string.IsNullOrWhiteSpace(oldKey))
            {
                try { await _imageStorage.DeleteFileAsync(oldKey); } catch { /* ignore */ }
            }
        }

        var (key, url) = await _imageStorage.UploadPublicImageAsync(content, fileName, contentType);
        restaurant.ImageUrl = url;
        await _restaurantRepository.UpdateRestaurantAsync(restaurant);
        return url;
    }

    private static string? ExtractKeyFromS3Url(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.TrimStart('/');
            var segments = path.Split('/', 2);
            if (uri.Host.StartsWith("s3.", StringComparison.OrdinalIgnoreCase) && segments.Length == 2)
                return segments[1];
            return path;
        }
        catch
        {
            return null;
        }
    }

    private static void ValidateAvailabilityArguments(DateTime? time, int? durationMinutes)
    {
        if (time.HasValue && !durationMinutes.HasValue)
            throw new ArgumentException("Query parameter 'duration' is required when 'time' is provided.");

        if (!time.HasValue && durationMinutes.HasValue)
            throw new ArgumentException("Query parameter 'time' is required when 'duration' is provided.");
    }

    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime.ToUniversalTime()
        };
    }
}

