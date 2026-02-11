namespace Application.Abstractions.Storage;

public interface IObjectStorage
{
    Task PutAsync(string key, Stream content, string contentType, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);

    // Optional helpers
    Task<bool> ExistsAsync(string key, CancellationToken ct);
    string GetPublicUrl(string key);
}
