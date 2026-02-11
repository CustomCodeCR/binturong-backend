using Application.Abstractions.Storage;

namespace Infrastructure.Storage;

public sealed class DefaultObjectStorageKeyBuilder : IObjectStorageKeyBuilder
{
    public string Build(string module, Guid aggregateId, string fileName)
    {
        var safeName = SanitizeFileName(fileName);
        return $"{module.Trim().ToLowerInvariant()}/{aggregateId:N}/{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}_{safeName}";
    }

    private static string SanitizeFileName(string name)
    {
        name = (name ?? string.Empty).Trim();
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }
}
