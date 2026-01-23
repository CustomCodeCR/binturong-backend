using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public interface IMongoMigration
{
    int Version { get; }
    string Name { get; }
    Task UpAsync(IMongoDatabase db, CancellationToken ct = default);
}
