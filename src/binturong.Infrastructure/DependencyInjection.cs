using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Outbox;
using Application.Abstractions.Projections;
using Application.Abstractions.Security;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Database.Mongo;
using Infrastructure.Database.Mongo.Migrations;
using Infrastructure.Database.Outbox;
using Infrastructure.Database.Postgres;
using Infrastructure.Database.Postgres.Seed;
using Infrastructure.Messaging;
using Infrastructure.Security;
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
        services.AddCoreServices();
        services.AddDatabase(configuration);
        services.AddMongo(configuration);
        services.AddOutboxAndProjections(configuration);
        services.AddHealthChecks(configuration);

        services.AddAuthenticationInternal(configuration);
        services.AddAuthorizationInternal();

        // =========================
        // Security seeders
        // =========================
        services.AddScoped<ScopeSeeder>();
        services.AddScoped<RoleSeeder>();
        services.AddScoped<AdminUserSeeder>();

        // =========================
        // Security services (used by API filters + Login)
        // =========================
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAdminPasswordResetService, AdminPasswordResetService>();

        // =========================
        // Auth services
        // =========================
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // If you still use these internally elsewhere, keep them:
        services.AddSingleton<ITokenProvider, TokenProvider>();

        // Current user (reads HttpContext.User claims)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<
            Application.Abstractions.Web.IRequestContext,
            Infrastructure.Web.RequestContext
        >();

        services.AddScoped<ICommandBus, CommandBus>();

        return services;
    }

    // =========================
    // Core services
    // =========================
    private static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }

    // =========================
    // Postgres (Write model)
    // =========================
    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    connectionString,
                    npgsql =>
                        npgsql.MigrationsHistoryTable(
                            HistoryRepository.DefaultTableName,
                            Schemas.Default
                        )
                )
                .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    // =========================
    // Mongo (Read model)
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

        // MongoDB.Driver 3.x: store Guid as UUID Standard
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.RegisterSerializer(
                typeof(Guid),
                new GuidSerializer(GuidRepresentation.Standard)
            );
        }
        catch
        {
            // ignore if already registered
        }

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));

        services.AddSingleton<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbName)
        );

        services.AddSingleton<IReadDb>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoReadDb(client, mongoDbName);
        });

        services.AddSingleton<MongoIndexSeeder>();
        services.AddSingleton<MongoMigrationRunner>();
        services.AddSingleton<IMongoBootstrapper, MongoBootstrapper>();

        return services;
    }

    // =========================
    // Outbox + projections
    // =========================
    private static IServiceCollection AddOutboxAndProjections(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));

        services.AddSingleton<IOutboxSerializer, OutboxSerializer>();
        services.AddSingleton<IOutboxMessageFactory, OutboxMessageFactory>();

        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(DependencyInjection))
                .AddClasses(c => c.AssignableTo(typeof(IProjector<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<IProjectionDispatcher, ProjectionDispatcher>();

        if (configuration.GetValue("Outbox:Enabled", true))
            services.AddHostedService<OutboxProcessor>();

        return services;
    }

    // =========================
    // Health checks
    // =========================
    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var postgresConn = configuration.GetConnectionString("Database");
        if (!string.IsNullOrWhiteSpace(postgresConn))
            services.AddHealthChecks().AddNpgSql(postgresConn);
        else
            services.AddHealthChecks();

        return services;
    }

    // =========================
    // Authentication (JWT)
    // =========================
    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSecret =
            configuration["Jwt:Secret"]
            ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Missing Jwt:Secret (or Jwt:Key)");

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

        return services;
    }

    // =========================
    // Authorization (Policies, if you use them)
    // =========================
    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<
            IAuthorizationPolicyProvider,
            PermissionAuthorizationPolicyProvider
        >();

        return services;
    }
}
