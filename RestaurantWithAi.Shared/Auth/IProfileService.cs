using System.IO;

namespace RestaurantWithAi.Shared.Auth;

public interface IProfileService
{
    Task<(string Key, string PresignedUrl)> UploadProfileImageAsync(string userId, Stream content, string fileName, string contentType, TimeSpan expiration);
}

