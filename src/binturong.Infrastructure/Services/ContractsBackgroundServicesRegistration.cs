using System.Threading.Channels;
using Application.Abstractions.Background;
using Application.Abstractions.Notifications;
using Application.Options;
using Infrastructure.Background;
using Infrastructure.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class ContractsBackgroundServicesRegistration
{
    public static IServiceCollection AddContractsBackground(
        this IServiceCollection services,
        EmailOptions emailOptions
    )
    {
        services.AddSingleton(emailOptions);

        var channel = Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();
        services.AddSingleton(channel);

        services.AddSingleton<IBackgroundJobScheduler>(sp => new InMemoryBackgroundJobScheduler(
            sp.GetRequiredService<Channel<Func<IServiceProvider, CancellationToken, Task>>>()
        ));

        services.AddHostedService(sp => new BackgroundJobWorker(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<Channel<Func<IServiceProvider, CancellationToken, Task>>>()
        ));

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IRealtimeNotifier, SignalRNotifier>();

        services.AddHostedService<ContractRenewalWorker>();

        return services;
    }
}
