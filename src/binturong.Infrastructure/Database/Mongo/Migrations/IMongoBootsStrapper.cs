namespace Infrastructure.Database.Mongo.Migrations;

public interface IMongoBootstrapper
{
    Task ApplyAsync(CancellationToken ct = default);
}

public sealed class MongoBootstrapper : IMongoBootstrapper
{
    private readonly MongoIndexSeeder _indexSeeder;
    private readonly MongoMigrationRunner _migrationRunner;

    public MongoBootstrapper(MongoIndexSeeder indexSeeder, MongoMigrationRunner migrationRunner)
    {
        _indexSeeder = indexSeeder;
        _migrationRunner = migrationRunner;
    }

    public async Task ApplyAsync(CancellationToken ct = default)
    {
        // 1) Ensure indexes
        await _indexSeeder.SeedAsync(ct);

        // 2) Apply versioned migrations (backfills, renames, etc.)
        await _migrationRunner.RunAsync(ct);
    }
}
