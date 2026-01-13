using Application.Abstractions.Data;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo;

public sealed class MongoReadDb : IReadDb
{
    public IMongoDatabase Database { get; }

    public MongoReadDb(IMongoClient client, string databaseName)
    {
        Database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<TReadModel> Collection<TReadModel>(string collectionName) =>
        Database.GetCollection<TReadModel>(collectionName);
}
