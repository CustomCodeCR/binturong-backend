using Application.Abstractions.Storage;
using Application.Options;

namespace Infrastructure.Storage;

public sealed class LocalObjectStorage : IObjectStorage
{
    private readonly string _rootPath;
    private readonly string _publicBaseUrl;

    public LocalObjectStorage(StorageOptions opt)
    {
        _rootPath = opt.Local.RootPath;
        _publicBaseUrl = opt.Local.PublicBaseUrl ?? string.Empty;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task PutAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        var absPath = GetAbsolutePath(key);
        var dir = Path.GetDirectoryName(absPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        if (content.CanSeek)
            content.Position = 0;

        await using var fs = new FileStream(
            absPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );
        await content.CopyToAsync(fs, ct);
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        var absPath = GetAbsolutePath(key);
        if (File.Exists(absPath))
            File.Delete(absPath);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        var absPath = GetAbsolutePath(key);
        return Task.FromResult(File.Exists(absPath));
    }

    public string GetPublicUrl(string key)
    {
        if (string.IsNullOrWhiteSpace(_publicBaseUrl))
            return key;

        return $"{_publicBaseUrl.TrimEnd('/')}/{key.TrimStart('/')}";
    }

    private string GetAbsolutePath(string key)
    {
        key = key.Replace("/", Path.DirectorySeparatorChar.ToString());
        return Path.Combine(_rootPath, key);
    }
}
