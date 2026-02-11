using Application.Abstractions.Background;

namespace Infrastructure.Background;

public sealed class BackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IBackgroundTaskQueue _queue;

    public BackgroundJobScheduler(IBackgroundTaskQueue queue) => _queue = queue;

    public Task EnqueueAsync(
        Func<IServiceProvider, CancellationToken, Task> job,
        CancellationToken ct
    ) => _queue.EnqueueAsync(job, ct).AsTask();
}
