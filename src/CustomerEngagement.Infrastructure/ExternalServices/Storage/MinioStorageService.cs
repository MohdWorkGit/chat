using Amazon.S3;
using Amazon.S3.Model;
using CustomerEngagement.Application.BackgroundJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Infrastructure.ExternalServices.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IConfiguration configuration, ILogger<MinioStorageService> logger)
    {
        _logger = logger;

        var endpoint = configuration["S3_ENDPOINT"] ?? "http://localhost:9000";
        var accessKey = configuration["S3_ACCESS_KEY"] ?? throw new InvalidOperationException("S3_ACCESS_KEY is not configured.");
        var secretKey = configuration["S3_SECRET_KEY"] ?? throw new InvalidOperationException("S3_SECRET_KEY is not configured.");
        _bucketName = configuration["S3_BUCKET"] ?? "customer-engagement";

        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            UseHttp = endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadFileAsync(
        string key,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        _logger.LogInformation("Uploaded file {Key} to bucket {Bucket}", key, _bucketName);

        return key;
    }

    public async Task<Stream> DownloadFileAsync(string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        _logger.LogDebug("Downloaded file {Key} from bucket {Bucket}", key, _bucketName);

        return response.ResponseStream;
    }

    public async Task DeleteFileAsync(string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
        _logger.LogInformation("Deleted file {Key} from bucket {Bucket}", key, _bucketName);
    }

    public string GeneratePresignedUrl(string key, TimeSpan? expiration = null)
    {
        var expiryDuration = expiration ?? TimeSpan.FromHours(1);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiryDuration),
            Verb = HttpVerb.GET
        };

        var url = _s3Client.GetPreSignedURL(request);
        _logger.LogDebug("Generated presigned URL for {Key}, expires in {Expiration}", key, expiryDuration);

        return url;
    }

    public string? GetFileUrl(string? keyOrUrl, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(keyOrUrl))
            return null;

        if (keyOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || keyOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return keyOrUrl;
        }

        return GeneratePresignedUrl(keyOrUrl, expiration ?? TimeSpan.FromHours(6));
    }

    public string GeneratePresignedUploadUrl(string key, string contentType, TimeSpan? expiration = null)
    {
        var expiryDuration = expiration ?? TimeSpan.FromMinutes(30);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiryDuration),
            Verb = HttpVerb.PUT,
            ContentType = contentType
        };

        var url = _s3Client.GetPreSignedURL(request);
        _logger.LogDebug("Generated presigned upload URL for {Key}", key);

        return url;
    }

    public async Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _s3Client.EnsureBucketExistsAsync(_bucketName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not ensure bucket {Bucket} exists. It may already exist.", _bucketName);
        }
    }
}
