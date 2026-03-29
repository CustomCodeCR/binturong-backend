using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Application.Abstractions.Authentication;
using Application.Abstractions.Background;
using Application.Abstractions.Data;
using Application.Abstractions.Documents;
using Application.Abstractions.EInvoicing;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Outbox;
using Application.Abstractions.Projections;
using Application.Abstractions.Security;
using Application.Abstractions.Storage;
using Application.Options;
using DinkToPdf;
using DinkToPdf.Contracts;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Background;
using Infrastructure.Database.Mongo;
using Infrastructure.Database.Mongo.Migrations;
using Infrastructure.Database.Outbox;
using Infrastructure.Database.Postgres;
using Infrastructure.Database.Postgres.Seed;
using Infrastructure.Documents;
using Infrastructure.EInvoicing;
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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SharedKernel;

namespace Infrastructure
{
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
            services.AddEInvoicing(configuration);
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
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<PayrollOptions>>().Value);

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
            services.Configure<StorageOptions>(config.GetSection("Storage"));
            services.Configure<EmailOptions>(config.GetSection("Email"));

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<StorageOptions>>().Value);

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailOptions>>().Value);

            services.AddSingleton<LocalObjectStorage>();

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var storageOptions = sp.GetRequiredService<StorageOptions>();
                var s3 = storageOptions.S3;

                var regionName = !string.IsNullOrWhiteSpace(s3.Region)
                    ? s3.Region
                    : config["AWS:Region"] ?? config["AWS_REGION"] ?? "us-east-1";

                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(regionName),
                };

                if (!string.IsNullOrWhiteSpace(s3.ServiceUrl))
                {
                    s3Config.ServiceURL = s3.ServiceUrl;
                    s3Config.ForcePathStyle = s3.ForcePathStyle;
                    s3Config.UseHttp = s3.ServiceUrl.StartsWith(
                        "http://",
                        StringComparison.OrdinalIgnoreCase
                    );

                    if (!string.IsNullOrWhiteSpace(s3.Region))
                        s3Config.AuthenticationRegion = s3.Region;
                }

                if (!string.IsNullOrWhiteSpace(s3.AccessKey))
                {
                    var credentials = new BasicAWSCredentials(
                        s3.AccessKey,
                        s3.SecretKey ?? string.Empty
                    );

                    return new AmazonS3Client(credentials, s3Config);
                }

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

            services.AddSingleton<IConverter>(_ => new SynchronizedConverter(new PdfTools()));
            services.AddScoped<IPdfGenerator, DinkToPdfGenerator>();
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
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSecret)
                        ),
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = !string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]),
                        ValidateAudience = !string.IsNullOrWhiteSpace(
                            configuration["Jwt:Audience"]
                        ),
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

        private static IServiceCollection AddEInvoicing(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddScoped<IElectronicInvoicingService, ElectronicInvoicingService>();

            services.AddSingleton<IExternalServiceHealth, StubExternalServiceHealth>();
            services.AddSingleton<IConsecutiveGenerator, StubConsecutiveGenerator>();
            services.AddSingleton<ITaxKeyGenerator, StubTaxKeyGenerator>();
            services.AddSingleton<IHaciendaClient, StubHaciendaClient>();
            services.AddSingleton<IDocumentStorage, StubDocumentStorage>();
            services.AddSingleton<IElectronicDocumentRenderer, StubElectronicDocumentRenderer>();

            return services;
        }
    }
}

namespace Infrastructure.EInvoicing
{
    using System.Text;
    using Application.Abstractions.EInvoicing;

    internal sealed class StubExternalServiceHealth : IExternalServiceHealth
    {
        public Task<bool> IsHaciendaUpAsync(CancellationToken ct) => Task.FromResult(true);
    }

    internal sealed class StubConsecutiveGenerator : IConsecutiveGenerator
    {
        public Task<string> NextAsync(string docType, CancellationToken ct) =>
            Task.FromResult($"STUB-{docType}-{DateTime.UtcNow:yyyyMMddHHmmss}");
    }

    internal sealed class StubTaxKeyGenerator : ITaxKeyGenerator
    {
        public Task<string> GenerateAsync(string docType, CancellationToken ct) =>
            Task.FromResult($"TK-{docType}-{Guid.NewGuid():N}".ToUpperInvariant());
    }

    internal sealed class StubHaciendaClient : IHaciendaClient
    {
        public Task<HaciendaSubmitResult> SubmitAsync(
            HaciendaSubmitRequest req,
            CancellationToken ct
        )
        {
            return Task.FromResult(new HaciendaSubmitResult(true, "Accepted", "OK"));
        }
    }

    internal sealed class StubDocumentStorage : IDocumentStorage
    {
        public Task<StoredDocument> PutAsync(
            string key,
            string contentType,
            byte[] bytes,
            CancellationToken ct
        )
        {
            return Task.FromResult(new StoredDocument(key, bytes.LongLength));
        }
    }

    internal sealed class StubElectronicDocumentRenderer : IElectronicDocumentRenderer
    {
        public Task<RenderedElectronicDocument> RenderInvoiceAsync(
            Guid invoiceId,
            CancellationToken ct
        ) => Task.FromResult(Build("invoice", invoiceId));

        public Task<RenderedElectronicDocument> RenderCreditNoteAsync(
            Guid creditNoteId,
            CancellationToken ct
        ) => Task.FromResult(Build("credit_note", creditNoteId));

        public Task<RenderedElectronicDocument> RenderDebitNoteAsync(
            Guid debitNoteId,
            CancellationToken ct
        ) => Task.FromResult(Build("debit_note", debitNoteId));

        private static RenderedElectronicDocument Build(string prefix, Guid id)
        {
            var xmlBytes = Encoding.UTF8.GetBytes($"<xml><doc>{prefix}</doc><id>{id}</id></xml>");
            var pdfBytes = Encoding.UTF8.GetBytes($"%PDF-1.4\n% Stub PDF for {prefix}:{id}\n");

            return new RenderedElectronicDocument(
                $"{prefix}_{id:N}.xml",
                $"{prefix}_{id:N}.pdf",
                "application/xml",
                "application/pdf",
                xmlBytes,
                pdfBytes
            );
        }
    }
}
