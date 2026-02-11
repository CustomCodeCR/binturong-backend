namespace Application.Abstractions.Background;

public interface IBackgroundJobScheduler
{
    Task EnqueueAsync(Func<IServiceProvider, CancellationToken, Task> job, CancellationToken ct);
}
