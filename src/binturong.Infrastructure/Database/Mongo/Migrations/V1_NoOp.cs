using MongoDB.Driver;

namespace Infrastructure.Database.Mongo.Migrations;

public sealed class V1_NoOp : IMongoMigration
{
    public int Version => 1;
    public string Name => "initial_noop";

    public Task UpAsync(IMongoDatabase db, CancellationToken ct = default) => Task.CompletedTask;
}
