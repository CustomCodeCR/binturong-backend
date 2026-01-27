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
            .AddOutboxAndProjections(configuration)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    // =========================
    // Services
    // =========================
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Collect domain events and write them into OutboxMessages
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }

    // =========================
    // Postgres (Write Model)
    // =========================
    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
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

        // Write abstraction
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );

        // NOTE:
        // Do NOT register DbContext as Scoped for HostedService consumption directly.
        // If OutboxProcessor needs DbContext, it MUST create scopes internally.
        // Still, keeping this mapping is okay for scoped consumers.
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    // =========================
    // Mongo (Read Model)
    // =========================
    private static IServiceCollection AddMongo(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var mongoConn =
            configuration.GetConnectionString("Mongo")
            ?? configuration["Mongo:ConnectionString"]
            ?? "mongodb://localhost:27017";

        var mongoDbName = configuration["Mongo:Database"] ?? "binturong_read";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));

        services.AddSingleton<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbName)
        );

        // Read abstraction
        services.AddSingleton<IReadDb>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoReadDb(client, mongoDbName);
        });

        // Mongo bootstrap components
        services.AddSingleton<MongoIndexSeeder>();

        // Versioned migrations runner
        services.AddSingleton<MongoMigrationRunner>();

        // One public entrypoint to apply Mongo indexes + migrations
        services.AddSingleton<IMongoBootstrapper, MongoBootstrapper>();

        return services;
    }

    // =========================
    // Outbox + Projections
    // =========================
    private static IServiceCollection AddOutboxAndProjections(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<IOutboxSerializer, OutboxSerializer>();
        services.AddSingleton<IOutboxMessageFactory, OutboxMessageFactory>();

        // IMPORTANT:
        // ProjectionDispatcher typically resolves IProjector<T> which you registered as Scoped in Application.
        // So Dispatcher should be Scoped too (or it must create scopes).
        services.AddScoped<IProjectionDispatcher, ProjectionDispatcher>();

        // Background worker: must use IServiceScopeFactory internally
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    // =========================
    // Health Checks
    // =========================
    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var postgresConn = configuration.GetConnectionString("Database");
        if (!string.IsNullOrWhiteSpace(postgresConn))
        {
            services.AddHealthChecks().AddNpgSql(postgresConn);
        }
        else
        {
            services.AddHealthChecks();
        }

        // Mongo health check optional (only if you added the package)
        // var mongoConn = configuration.GetConnectionString("Mongo");
        // if (!string.IsNullOrWhiteSpace(mongoConn))
        //     services.AddHealthChecks().AddMongoDb(mongoConn);

        return services;
    }

    // =========================
    // AuthN
    // =========================
    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSecret =
            configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Missing Jwt:Secret in configuration.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = !string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]),
                    ValidateAudience = !string.IsNullOrWhiteSpace(configuration["Jwt:Audience"]),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    // =========================
    // AuthZ
    // =========================
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
