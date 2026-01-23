using Application;
using Infrastructure;
using Infrastructure.Database.Mongo.Migrations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();
    await app.ApplyMongoAsync();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/weatherforecast", () => "OK");

app.Run();

// ===== Helpers =====
static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db =
            scope.ServiceProvider.GetRequiredService<Infrastructure.Database.Postgres.ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public static async Task ApplyMongoAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        // 1) Ensure indexes
        var indexSeeder = scope.ServiceProvider.GetRequiredService<MongoIndexSeeder>();
        await indexSeeder.SeedAsync();

        // 2) Run versioned migrations (data backfills / renames)
        var runner = scope.ServiceProvider.GetRequiredService<MongoMigrationRunner>();
        await runner.RunAsync();
    }
}
