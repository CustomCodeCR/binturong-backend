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
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddServices();
        services.AddDatabase(configuration);
        services.AddMongo(configuration);
        services.AddOutboxAndProjections(configuration);
        services.AddHealthChecks(configuration);
        services.AddAuthenticationInternal(configuration);
        services.AddAuthorizationInternal();

        return services;
    }

    // =========================
    // Services
    // =========================
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
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

        // Abstraction used by Application
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );

        // OutboxProcessor asks DbContext (scoped) inside a scope
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
        // Prefer ConnectionStrings:Mongo; fallback to Mongo:ConnectionString
        var mongoConn =
            configuration.GetConnectionString("Mongo")
            ?? configuration["Mongo:ConnectionString"]
            ?? "mongodb://localhost:27017";

        // Database name for read models
        var mongoDbName = configuration["Mongo:Database"] ?? "binturong_read";

        // ===== Mongo GUID configuration (MongoDB.Driver 3.x) =====
        // GuidRepresentationConvention no longer exists in 3.x.
        // Configure GUID storage via serializers (UUID Standard / subtype 4).
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // Optional: helps when Guids are stored inside "object" fields
            var disc = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
            BsonSerializer.RegisterSerializer(
                new ObjectSerializer(disc, GuidRepresentation.Standard)
            );
        }
        catch
        {
            // ignore if already registered (tests / multiple startup runs)
        }

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));

        services.AddSingleton<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbName)
        );

        // IReadDb abstraction (if your app uses it)
        services.AddSingleton<IReadDb>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoReadDb(client, mongoDbName);
        });

        // Indexes + migrations
        services.AddSingleton<MongoIndexSeeder>();
        services.AddSingleton<MongoMigrationRunner>();

        // Single entrypoint used by Program.cs
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
        // Bind Outbox options from config ("Outbox": { ... })
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));

        services.AddSingleton<IOutboxSerializer, OutboxSerializer>();
        services.AddSingleton<IOutboxMessageFactory, OutboxMessageFactory>();

        // Register all IProjector<> from Infrastructure assembly (includes UserProjection, etc.)
        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(DependencyInjection))
                .AddClasses(c => c.AssignableTo(typeof(IProjector<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // Dispatcher must be scoped because it resolves scoped projectors
        services.AddScoped<IProjectionDispatcher, ProjectionDispatcher>();

        // HostedService should only run if enabled
        if (configuration.GetValue("Outbox:Enabled", true))
        {
            services.AddHostedService<OutboxProcessor>();
        }

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

        // Optional: Mongo health checks if you add the package
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
            ?? throw new InvalidOperationException("Missing Jwt:Secret");

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
