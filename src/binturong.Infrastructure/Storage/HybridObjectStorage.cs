using Application.Abstractions.Storage;
using Application.Options;

namespace Infrastructure.Storage;

public sealed class HybridObjectStorage : IObjectStorage
{
    private readonly StorageOptions _opt;
    private readonly IObjectStorage _s3;
    private readonly IObjectStorage _local;

    public HybridObjectStorage(StorageOptions opt, S3ObjectStorage s3, LocalObjectStorage local)
    {
        _opt = opt;
        _s3 = s3;
        _local = local;
    }

    public Task PutAsync(string key, Stream content, string contentType, CancellationToken ct) =>
        UseS3()
            ? _s3.PutAsync(key, content, contentType, ct)
            : _local.PutAsync(key, content, contentType, ct);

    public Task DeleteAsync(string key, CancellationToken ct) =>
        UseS3() ? _s3.DeleteAsync(key, ct) : _local.DeleteAsync(key, ct);

    public Task<bool> ExistsAsync(string key, CancellationToken ct) =>
        UseS3() ? _s3.ExistsAsync(key, ct) : _local.ExistsAsync(key, ct);

    public string GetPublicUrl(string key) =>
        UseS3() ? _s3.GetPublicUrl(key) : _local.GetPublicUrl(key);

    private bool UseS3() => _opt.S3.Enabled && !string.IsNullOrWhiteSpace(_opt.S3.Bucket);
}
