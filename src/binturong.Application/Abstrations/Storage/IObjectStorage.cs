namespace Application.Abstractions.Storage;

public interface IObjectStorage
{
    Task PutAsync(string key, Stream content, string contentType, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
    Task<bool> ExistsAsync(string key, CancellationToken ct);

    Task<string> GetReadUrlAsync(string key, CancellationToken ct);

    // Optional helper
    string GetPublicUrl(string key);
}
