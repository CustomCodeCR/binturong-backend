using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string MigrationsCollectionName = "__migrations";
    private const string DocumentId = "mongo_migrations"; // one doc to store version

    private readonly IMongoDatabase _database;

    public MongoMigrationRunner(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var migrationsCollection = _database.GetCollection<BsonDocument>(MigrationsCollectionName);

        // 1) Read current version (no LINQ)
        var filter = Builders<BsonDocument>.Filter.Eq("_id", DocumentId);

        var projection = Builders<BsonDocument>.Projection.Include("version");

        var state = await migrationsCollection
            .Find(filter)
            .Project(projection)
            .FirstOrDefaultAsync(ct);

        var currentVersion = 0;

        if (state is not null && state.TryGetValue("version", out var v))
        {
            // version can be int32/int64/double depending how it was written
            currentVersion =
                v.IsInt32 ? v.AsInt32
                : v.IsInt64 ? (int)v.AsInt64
                : v.IsDouble ? (int)v.AsDouble
                : 0;
        }

        // 2) Run migrations in order
        // TODO: replace with your real migrations
        var migrations = new List<IMongoMigration>
        {
            // new V1_Something(),
            // new V2_SomethingElse(),
        };

        foreach (var migration in migrations.OrderBy(m => m.Version))
        {
            if (migration.Version <= currentVersion)
                continue;

            await migration.UpAsync(_database, ct);

            // 3) Persist new version atomically
            var update = Builders<BsonDocument>
                .Update.Set("version", migration.Version)
                .Set("updatedAt", DateTime.UtcNow);

            await migrationsCollection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true },
                ct
            );

            currentVersion = migration.Version;
        }
    }
}

// Simple contract for versioned migrations
public interface IMongoMigration
{
    int Version { get; }
    Task UpAsync(IMongoDatabase db, CancellationToken ct);
}
