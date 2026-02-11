namespace Application.Abstractions.Storage;

public interface IObjectStorageKeyBuilder
{
    string Build(string module, Guid aggregateId, string fileName);
}
