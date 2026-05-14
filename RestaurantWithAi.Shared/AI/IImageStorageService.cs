using System.IO;

namespace RestaurantWithAi.Shared.AI;

public interface IImageStorageService
{
    // Upload a private image and return the object key and a presigned URL for access
    Task<(string Key, string PresignedUrl)> UploadPrivateImageAsync(Stream content, string fileName, string contentType, TimeSpan expiration);

    // Upload a public image and return the object key and the public URL
    Task<(string Key, string Url)> UploadPublicImageAsync(Stream content, string fileName, string contentType);

    // Delete a file by its S3 object key
    Task DeleteFileAsync(string fileKey);
}

