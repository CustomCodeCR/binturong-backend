using System.Threading.Channels;
using Application.Abstractions.Background;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Background;

public sealed class InMemoryBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _ch;

    public InMemoryBackgroundJobScheduler(
        Channel<Func<IServiceProvider, CancellationToken, Task>> ch
    )
    {
        _ch = ch;
    }

    public Task EnqueueAsync(
        Func<IServiceProvider, CancellationToken, Task> job,
        CancellationToken ct
    ) => _ch.Writer.WriteAsync(job, ct).AsTask();
}

public sealed class BackgroundJobWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _ch;

    public BackgroundJobWorker(
        IServiceScopeFactory scopeFactory,
        Channel<Func<IServiceProvider, CancellationToken, Task>> ch
    )
    {
        _scopeFactory = scopeFactory;
        _ch = ch;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _ch.Reader.ReadAsync(stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            await job(scope.ServiceProvider, stoppingToken);
        }
    }
}
