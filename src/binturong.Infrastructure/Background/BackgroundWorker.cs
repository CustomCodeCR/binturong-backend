using Application.Abstractions.Background;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background;

public sealed class BackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<BackgroundWorker> _logger;

    public BackgroundWorker(
        IServiceProvider sp,
        IBackgroundTaskQueue queue,
        ILogger<BackgroundWorker> logger
    )
    {
        _sp = sp;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Func<IServiceProvider, CancellationToken, Task> workItem;

            try
            {
                workItem = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await workItem(_sp, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background job failed.");
            }
        }
    }
}
