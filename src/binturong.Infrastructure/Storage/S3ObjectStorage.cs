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
                : contentType,
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

    public string GetPublicUrl(string key)
    {
        // If you use CloudFront, change this
        return $"https://{_opt.S3.Bucket}.s3.amazonaws.com/{NormalizeKey(key)}";
    }

    private string NormalizeKey(string key) => key.TrimStart('/');
}
