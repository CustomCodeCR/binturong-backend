using MongoDB.Driver;

namespace Application.Abstractions.Data;

public interface IReadDb
{
    IMongoDatabase Database { get; }

    IMongoCollection<TReadModel> Collection<TReadModel>(string collectionName);
}
