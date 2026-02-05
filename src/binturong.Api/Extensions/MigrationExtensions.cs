using Infrastructure.Database.Mongo.Migrations;
using Infrastructure.Database.Postgres;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyDevMigrationsAndMongoAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var mongo = scope.ServiceProvider.GetRequiredService<IMongoBootstrapper>();
        await mongo.ApplyAsync();
    }
}
