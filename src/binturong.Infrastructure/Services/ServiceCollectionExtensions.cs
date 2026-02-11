using Amazon;
using Amazon.S3;
using Application.Abstractions.Background;
using Application.Abstractions.Documents;
using Application.Abstractions.Notifications;
using Application.Abstractions.Storage;
using Application.Options;
using Infrastructure.Background;
using Infrastructure.Documents;
using Infrastructure.Notifications;
using Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformServices(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        // Options
        var storageOpt = config.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();
        var emailOpt = config.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();

        services.AddSingleton(storageOpt);
        services.AddSingleton(emailOpt);

        // Storage implementations
        services.AddSingleton<LocalObjectStorage>();

        // S3 client (no AWSSDK.Extensions.NETCore.Setup)
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var regionName =
                storageOpt.S3?.Region
                ?? config["AWS:Region"]
                ?? config["AWS_REGION"]
                ?? "us-east-1";

            var region = RegionEndpoint.GetBySystemName(regionName);

            // Optional (LocalStack/MinIO) via config only:
            // Storage:S3:ServiceUrl = http://localhost:4566
            // Storage:S3:ForcePathStyle = true
            var serviceUrl =
                config["Storage:S3:ServiceUrl"]
                ?? config["AWS:ServiceURL"]
                ?? config["AWS_SERVICE_URL"];

            var forcePathStyle =
                bool.TryParse(config["Storage:S3:ForcePathStyle"], out var fps) && fps;

            var s3Config = new AmazonS3Config { RegionEndpoint = region };

            if (!string.IsNullOrWhiteSpace(serviceUrl))
            {
                s3Config.ServiceURL = serviceUrl;
                s3Config.ForcePathStyle = forcePathStyle;
            }

            // Credentials:
            // - Uses env vars AWS_ACCESS_KEY_ID/AWS_SECRET_ACCESS_KEY automatically if present
            // - Otherwise default chain (shared credentials file, IAM role, etc.)
            return new AmazonS3Client(s3Config);
        });

        services.AddSingleton<S3ObjectStorage>();

        // Hybrid storage as the IObjectStorage
        services.AddScoped<IObjectStorage, HybridObjectStorage>();
        services.AddSingleton<IObjectStorageKeyBuilder, DefaultObjectStorageKeyBuilder>();

        // Background
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IBackgroundJobScheduler, BackgroundJobScheduler>();
        services.AddHostedService<BackgroundWorker>();

        // Notifications
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IRealtimeNotifier, SignalRNotifier>();

        // Documents
        services.AddScoped<IPdfGenerator, IronPdfGenerator>();
        services.AddScoped<IExcelExporter, ClosedXmlExcelExporter>();

        return services;
    }
}
