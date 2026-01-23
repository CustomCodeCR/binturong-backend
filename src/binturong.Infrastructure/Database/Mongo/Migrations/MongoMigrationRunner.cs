using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class MongoMigrationRunner
{
    private const string HistoryCollection = "_mongo_migrations";
    private readonly IMongoDatabase _db;
    private readonly IReadOnlyList<IMongoMigration> _migrations;

    public MongoMigrationRunner(IMongoDatabase db, IEnumerable<IMongoMigration> migrations)
    {
        _db = db;
        _migrations = migrations.OrderBy(m => m.Version).ToList();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var history = _db.GetCollection<BsonDocument>(HistoryCollection);

        // Ensure unique version index (so we never apply same version twice)
        await history.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("version"),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        );

        var appliedVersions = await history
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Project(d => d["version"].AsInt32)
            .ToListAsync(ct);

        foreach (var migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db, ct);

            var doc = new BsonDocument
            {
                ["version"] = migration.Version,
                ["name"] = migration.Name,
                ["executedAtUtc"] = DateTime.UtcNow,
            };

            await history.InsertOneAsync(doc, cancellationToken: ct);
        }
    }
}
