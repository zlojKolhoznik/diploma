using AutoMapper;
using Microsoft.Extensions.Logging;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Dishes;

namespace RestaurantWithAi.Core.Services;

public class DishService : IDishesService
{
    private readonly IDishRepository _dishRepository;
    private readonly AutoMapper.IMapper _mapper;
    private readonly IImageStorageService? _imageStorage;
    private readonly Microsoft.Extensions.Logging.ILogger<DishService>? _logger;

    public DishService(IDishRepository dishRepository, AutoMapper.IMapper mapper)
        : this(dishRepository, mapper, null, null)
    {
    }

    public DishService(IDishRepository dishRepository, AutoMapper.IMapper mapper, IImageStorageService? imageStorage, Microsoft.Extensions.Logging.ILogger<DishService>? logger)
    {
        _dishRepository = dishRepository;
        _mapper = mapper;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<IEnumerable<DishBrief>> GetDishesAsync()
    {
        var dishes = await _dishRepository.GetAllDishesAsync();
        return _mapper.Map<IEnumerable<DishBrief>>(dishes);
    }

    public async Task<DishDetail> GetDishDetailAsync(Guid id)
    {
        var dish = await _dishRepository.GetDishByIdAsync(id);
        return _mapper.Map<DishDetail>(dish);
    }

    public async Task CreateDishAsync(CreateDishRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var dish = _mapper.Map<Dish>(request);
        await _dishRepository.AddDishAsync(dish);
    }

    public async Task UpdateDishAsync(Guid id, CreateDishRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var dish = _mapper.Map<Dish>(request);
        dish.Id = id;
        await _dishRepository.UpdateDishAsync(dish);
    }

    public Task UpdateDishAvailabilityAsync(Guid id, IEnumerable<Guid> restaurantIds)
    {
        ArgumentNullException.ThrowIfNull(restaurantIds);
        return _dishRepository.UpdateDishAvailabilityAsync(id, restaurantIds);
    }

    public Task DeleteDishAsync(Guid id)
    {
        return _dishRepository.DeleteDishAsync(id);
    }

    public async Task<string> UploadDishImageAsync(Guid id, System.IO.Stream content, string fileName, string contentType)
    {
        if (_imageStorage is null)
            throw new InvalidOperationException("Image storage is not configured.");

        var dish = await _dishRepository.GetDishByIdAsync(id);

        // Attempt to delete previous image if present
        if (!string.IsNullOrWhiteSpace(dish.ImageUrl))
        {
            var oldKey = ExtractKeyFromS3Url(dish.ImageUrl);
            if (!string.IsNullOrWhiteSpace(oldKey))
            {
                try { await _imageStorage.DeleteFileAsync(oldKey); } catch { /* swallow - non-fatal */ }
            }
        }

        var (key, url) = await _imageStorage.UploadPublicImageAsync(content, fileName, contentType);

        dish.ImageUrl = url;
        await _dishRepository.UpdateDishAsync(dish);

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
}