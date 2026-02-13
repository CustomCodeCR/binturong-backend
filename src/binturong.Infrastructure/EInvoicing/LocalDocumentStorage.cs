using Application.Abstractions.EInvoicing;

namespace Infrastructure.EInvoicing;

public sealed class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _root;

    public LocalDocumentStorage(string rootPath)
    {
        _root = string.IsNullOrWhiteSpace(rootPath) ? "storage/einvoicing" : rootPath;
        Directory.CreateDirectory(_root);
    }

    public async Task<StoredDocument> PutAsync(
        string key,
        string contentType,
        byte[] bytes,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("key is required", nameof(key));

        var safe = key.Replace("..", "").Replace("\\", "/").TrimStart('/');
        var full = Path.Combine(_root, safe.Replace("/", Path.DirectorySeparatorChar.ToString()));
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        await File.WriteAllBytesAsync(full, bytes, ct);
        return new StoredDocument(safe, bytes.LongLength);
    }
}
