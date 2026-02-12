using System.Text;
using Amazon;
using Amazon.S3;
using Application.Abstractions.Authentication;
using Application.Abstractions.Background;
using Application.Abstractions.Data;
using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Outbox;
using Application.Abstractions.Projections;
using Application.Abstractions.Security;
using Application.Abstractions.Storage;
using Application.Options;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Background;
using Infrastructure.Database.Mongo;
using Infrastructure.Database.Mongo.Migrations;
using Infrastructure.Database.Outbox;
using Infrastructure.Database.Postgres;
using Infrastructure.Database.Postgres.Seed;
using Infrastructure.Documents;
using Infrastructure.Messaging;
using Infrastructure.Notifications;
using Infrastructure.Security;
using Infrastructure.Storage;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

        services.AddPlatformServices(configuration);

        services.AddAuthenticationInternal(configuration);
        services.AddAuthorizationInternal();

        services.AddScoped<ScopeSeeder>();
        services.AddScoped<RoleSeeder>();
        services.AddScoped<AdminUserSeeder>();

        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAdminPasswordResetService, AdminPasswordResetService>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<
            Application.Abstractions.Web.IRequestContext,
            Infrastructure.Web.RequestContext
        >();

        services.AddScoped<ICommandBus, CommandBus>();

        services.Configure<PayrollOptions>(configuration.GetSection("Payroll"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PayrollOptions>>().Value
        );

        return services;
    }

    private static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }

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
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
        );

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

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

        try
        {
            BsonSerializer.RegisterSerializer(
                new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard)
            );
            BsonSerializer.RegisterSerializer(
                typeof(Guid),
                new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard)
            );
        }
        catch { }

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

    private static IServiceCollection AddPlatformServices(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var storageOpt = config.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();
        var emailOpt = config.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();

        services.AddSingleton(storageOpt);
        services.AddSingleton(emailOpt);

        services.AddSingleton<LocalObjectStorage>();

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var regionName = config["AWS:Region"] ?? config["AWS_REGION"] ?? "us-east-1";
            var region = RegionEndpoint.GetBySystemName(regionName);
            var s3Config = new AmazonS3Config { RegionEndpoint = region };
            return new AmazonS3Client(s3Config);
        });

        services.AddSingleton<S3ObjectStorage>();
        services.AddScoped<IObjectStorage, HybridObjectStorage>();
        services.AddSingleton<IObjectStorageKeyBuilder, DefaultObjectStorageKeyBuilder>();

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IBackgroundJobScheduler, BackgroundJobScheduler>();
        services.AddHostedService<BackgroundWorker>();
        services.AddHostedService<ContractRenewalWorker>();

        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IRealtimeNotifier, SignalRNotifier>();

        services.AddScoped<IPdfGenerator, IronPdfGenerator>();
        services.AddScoped<IExcelExporter, ClosedXmlExcelExporter>();

        return services;
    }

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
