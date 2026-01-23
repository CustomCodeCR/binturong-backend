using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Outbox;
using Application.Abstractions.Projections;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Database.Mongo;
using Infrastructure.Database.Mongo.Migrations;
using Infrastructure.Database.Outbox;
using Infrastructure.Database.Postgres;
using Infrastructure.DomainEvents;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    ) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddMongo(configuration)
            .AddOutboxAndProjections()
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // This dispatcher should collect domain events and write them into OutboxMessages.
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Postgres (WriteModel)
        var connectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Missing ConnectionStrings:Database in binturong.Api appsettings."
            );

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(
                            HistoryRepository.DefaultTableName,
                            Schemas.Default
                        )
                )
                .UseSnakeCaseNamingConvention()
        );

        // Your Write DbContext
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );

        // Needed because OutboxProcessor asks for DbContext
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddMongo(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Mongo (ReadModel)
        // Prefer ConnectionStrings:Mongo; fallback to "Mongo:ConnectionString"
        var mongoConn =
            configuration.GetConnectionString("Mongo")
            ?? configuration["Mongo:ConnectionString"]
            ?? "mongodb://localhost:27017";

        var mongoDbName = configuration["Mongo:Database"] ?? "binturong_read";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDbName);
        });

        // IReadDb abstraction
        services.AddSingleton<IReadDb>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoReadDb(client, mongoDbName);
        });

        // Index seeder (run from Program.cs on startup)
        services.AddSingleton<MongoIndexSeeder>();

        // Mongo migrations (versioned data migrations)
        // Register each migration here (or swap to assembly scanning later)
        // services.AddSingleton<IMongoMigration, V1_NoOp>();
        services.AddSingleton<MongoMigrationRunner>();

        return services;
    }

    private static IServiceCollection AddOutboxAndProjections(this IServiceCollection services)
    {
        // Outbox serialization + outbox message creation
        services.AddSingleton<IOutboxSerializer, OutboxSerializer>();
        services.AddSingleton<IOutboxMessageFactory, OutboxMessageFactory>();

        // Projection dispatcher (resolves IProjector<TEvent> via DI)
        services.AddSingleton<IProjectionDispatcher, ProjectionDispatcher>();

        // Background worker that reads OutboxMessages and projects to Mongo
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHealthChecks().AddNpgSql(configuration.GetConnectionString("Database")!);

        // If you have the package for Mongo health checks, you can add it here.
        // services.AddHealthChecks().AddMongoDb(configuration.GetConnectionString("Mongo")!);

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)
                    ),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<
            IAuthorizationPolicyProvider,
            PermissionAuthorizationPolicyProvider
        >();

        return services;
    }
}
