using System.Threading.Channels;
using Application.Abstractions.Background;

namespace Infrastructure.Background;

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );

    public async ValueTask EnqueueAsync(
        Func<IServiceProvider, CancellationToken, Task> workItem,
        CancellationToken ct
    )
    {
        if (workItem is null)
            throw new ArgumentNullException(nameof(workItem));
        await _queue.Writer.WriteAsync(workItem, ct);
    }

    public async ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(
        CancellationToken ct
    )
    {
        var item = await _queue.Reader.ReadAsync(ct);
        return item;
    }
}
