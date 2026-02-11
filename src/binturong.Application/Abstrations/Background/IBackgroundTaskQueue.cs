namespace Application.Abstractions.Background;

public interface IBackgroundTaskQueue
{
    ValueTask EnqueueAsync(
        Func<IServiceProvider, CancellationToken, Task> workItem,
        CancellationToken ct
    );
    ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct);
}
