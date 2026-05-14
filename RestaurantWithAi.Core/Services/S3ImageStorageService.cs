using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Options;

namespace RestaurantWithAi.Core.Services;

public class S3ImageStorageService : IImageStorageService
{
    private readonly AwsS3Options _options;
    private readonly IAmazonS3 _s3Client;

    public S3ImageStorageService(IOptions<AwsS3Options> options)
    {
        _options = options.Value;
        var creds = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
        var region = RegionEndpoint.GetBySystemName(_options.Region);
        _s3Client = new AmazonS3Client(creds, region);
    }

    public async Task<(string Key, string PresignedUrl)> UploadPrivateImageAsync(System.IO.Stream content, string fileName, string contentType, TimeSpan expiration)
    {
        var key = $"private/{Guid.NewGuid()}_{SanitizeFileName(fileName)}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            CannedACL = S3CannedACL.Private
        };

        var response = await _s3Client.PutObjectAsync(putRequest);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK && response.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
            throw new InvalidOperationException("Failed to upload file to S3.");

        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiration)
        };

        var url = _s3Client.GetPreSignedURL(presignRequest);
        return (key, url);
    }

    public async Task<(string Key, string Url)> UploadPublicImageAsync(System.IO.Stream content, string fileName, string contentType)
    {
        var key = $"public/{Guid.NewGuid()}_{SanitizeFileName(fileName)}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        var response = await _s3Client.PutObjectAsync(putRequest);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK && response.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
            throw new InvalidOperationException("Failed to upload file to S3.");

        // Construct public URL (region-aware)
        var regionSegment = string.Equals(_options.Region, "us-east-1", StringComparison.OrdinalIgnoreCase)
            ? "s3.amazonaws.com"
            : $"s3.{_options.Region}.amazonaws.com";
        var url = $"https://{_options.BucketName}.{regionSegment}/{Uri.EscapeDataString(key)}";
        return (key, url);
    }

    public async Task DeleteFileAsync(string fileKey)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
            return;

        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Already deleted - ignore
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return "file";
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');
        return fileName;
    }
}


