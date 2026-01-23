using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Database.Postgres;

internal sealed class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var current = Directory.GetCurrentDirectory();

        // If we're already in src/binturong.Api, appsettings.json is here.
        // Otherwise assume we're in repo root and appsettings is under src/binturong.Api
        var apiPath = File.Exists(Path.Combine(current, "appsettings.json"))
            ? current
            : Path.Combine(current, "src", "binturong.Api");

        var appsettingsPath = Path.Combine(apiPath, "appsettings.json");
        if (!File.Exists(appsettingsPath))
            throw new FileNotFoundException(
                $"Could not find appsettings.json. Current='{current}', ApiPath='{apiPath}'"
            );

        var environment =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            config.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Database");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder
            .UseNpgsql(
                connectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                }
            )
            .UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
