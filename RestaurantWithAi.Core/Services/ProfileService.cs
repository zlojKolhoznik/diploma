using System.IO;
using Microsoft.Extensions.Logging;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.AI;

namespace RestaurantWithAi.Core.Services;

public class ProfileService : IProfileService
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly IImageStorageService _imageStorage;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(IUserProfileRepository profileRepository, IImageStorageService imageStorage, ILogger<ProfileService> logger)
    {
        _profileRepository = profileRepository;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<(string Key, string PresignedUrl)> UploadProfileImageAsync(string userId, Stream content, string fileName, string contentType, TimeSpan expiration)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(content);

        // Fetch existing profile and delete old image if present
        var existing = await _profileRepository.GetByUserIdAsync(userId);
        if (existing is not null && !string.IsNullOrWhiteSpace(existing.PhotoStorageKey))
        {
            try
            {
                await _imageStorage.DeleteFileAsync(existing.PhotoStorageKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old profile image for user {UserId}", userId);
            }
        }

        // Upload new private image
        var (key, presignedUrl) = await _imageStorage.UploadPrivateImageAsync(content, fileName, contentType, expiration);

        var profile = new UserProfile { UserId = userId, PhotoStorageKey = key };
        await _profileRepository.UpsertAsync(profile);
        await _profileRepository.SaveChangesAsync();

        return (key, presignedUrl);
    }
}

