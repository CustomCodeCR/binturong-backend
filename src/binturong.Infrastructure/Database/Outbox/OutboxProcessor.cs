using Application.Abstractions.Outbox;
using Application.Abstractions.Projections;
using Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Outbox;

internal sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);
    private const int BatchSize = 50;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in OutboxProcessor.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<DbContext>();
        var serializer = scope.ServiceProvider.GetRequiredService<IOutboxSerializer>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IProjectionDispatcher>();

        var messages = await db.Set<OutboxMessage>()
            .Where(x =>
                x.Status == "PENDING"
                && (x.NextAttemptAt == null || x.NextAttemptAt <= DateTime.UtcNow)
            )
            .OrderBy(x => x.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        foreach (var msg in messages)
        {
            try
            {
                var domainEvent = serializer.Deserialize(msg.PayloadJson, msg.Type);

                await dispatcher.DispatchAsync(domainEvent, ct);

                msg.Status = "PUBLISHED";
                msg.Attempts++;
                msg.LastError = null;
                msg.NextAttemptAt = null;
            }
            catch (Exception ex)
            {
                msg.Attempts++;
                msg.Status = "FAILED";
                msg.LastError = ex.ToString();
                msg.NextAttemptAt = DateTime.UtcNow.AddMinutes(5);

                _logger.LogError(ex, "Failed processing outbox message {OutboxId}", msg.OutboxId);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
