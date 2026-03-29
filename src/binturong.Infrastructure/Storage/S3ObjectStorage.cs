using Amazon.S3;
using Amazon.S3.Model;
using Application.Abstractions.Storage;
using Application.Options;

namespace Infrastructure.Storage;

public sealed class S3ObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 _s3;
    private readonly StorageOptions _opt;

    public S3ObjectStorage(IAmazonS3 s3, StorageOptions opt)
    {
        _s3 = s3;
        _opt = opt;
    }

    public async Task PutAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        if (content.CanSeek)
            content.Position = 0;

        var req = new PutObjectRequest
        {
            BucketName = _opt.S3.Bucket,
            Key = NormalizeKey(key),
            InputStream = content,
            ContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType.Trim(),
        };

        await _s3.PutObjectAsync(req, ct);
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        var req = new DeleteObjectRequest { BucketName = _opt.S3.Bucket, Key = NormalizeKey(key) };

        await _s3.DeleteObjectAsync(req, ct);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        try
        {
            await _s3.GetObjectMetadataAsync(_opt.S3.Bucket, NormalizeKey(key), ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public Task<string> GetReadUrlAsync(string key, CancellationToken ct)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _opt.S3.Bucket,
            Key = NormalizeKey(key),
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        var url = _s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public string GetPublicUrl(string key)
    {
        var normalizedKey = NormalizeKey(key);

        if (!string.IsNullOrWhiteSpace(_opt.S3.ServiceUrl))
        {
            var baseUrl = _opt.S3.ServiceUrl.Trim().TrimEnd('/');

            if (_opt.S3.ForcePathStyle)
                return $"{baseUrl}/{_opt.S3.Bucket}/{normalizedKey}";

            var uri = new Uri(baseUrl);
            var hostWithBucket = $"{_opt.S3.Bucket}.{uri.Host}";
            var portPart = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
            return $"{uri.Scheme}://{hostWithBucket}{portPart}/{normalizedKey}";
        }

        if (!string.IsNullOrWhiteSpace(_opt.S3.Region))
            return $"https://{_opt.S3.Bucket}.s3.{_opt.S3.Region}.amazonaws.com/{normalizedKey}";

        return $"https://{_opt.S3.Bucket}.s3.amazonaws.com/{normalizedKey}";
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Storage key cannot be null or empty.", nameof(key));

        return key.Trim().TrimStart('/');
    }
}
