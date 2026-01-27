using Application.Abstractions.Outbox;
using Application.Abstractions.Projections;
using Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Outbox;

internal sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxOptions _options;

    private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(30);

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger,
        IOptions<OutboxOptions> options
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxProcessor started. PollingIntervalSeconds={PollingIntervalSeconds}, BatchSize={BatchSize}, MaxAttempts={MaxAttempts}",
            _options.PollingIntervalSeconds,
            _options.BatchSize,
            _options.MaxAttempts
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in OutboxProcessor loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<DbContext>();
        var serializer = scope.ServiceProvider.GetRequiredService<IOutboxSerializer>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IProjectionDispatcher>();

        var now = DateTime.UtcNow;

        // 1) Load candidates
        var candidates = await db.Set<OutboxMessage>()
            .Where(x =>
                x.Status == OutboxStatus.Pending
                && (x.NextAttemptAt == null || x.NextAttemptAt <= now)
            )
            .OrderBy(x => x.OccurredAt)
            .Take(_options.BatchSize)
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return;

        // 2) Claim messages (simple locking to avoid double-processing)
        foreach (var msg in candidates)
        {
            msg.Status = OutboxStatus.Processing;
            msg.LockedUntil = now.Add(LockDuration);
        }

        await db.SaveChangesAsync(ct);

        // 3) Process claimed
        foreach (var msg in candidates)
        {
            try
            {
                var domainEvent = serializer.Deserialize(msg.PayloadJson, msg.Type);

                await dispatcher.DispatchAsync(domainEvent, ct);

                msg.Status = OutboxStatus.Published;
                msg.Attempts++;
                msg.LastError = null;
                msg.NextAttemptAt = null;
            }
            catch (Exception ex)
            {
                msg.Attempts++;
                msg.LastError = ex.ToString();

                if (msg.Attempts >= _options.MaxAttempts)
                {
                    // Too many attempts => dead letter
                    msg.Status = OutboxStatus.Dead;
                    msg.NextAttemptAt = null;

                    _logger.LogError(
                        ex,
                        "Outbox message {OutboxId} moved to DEAD after {Attempts} attempts",
                        msg.Id,
                        msg.Attempts
                    );
                }
                else
                {
                    // Retry later
                    msg.Status = OutboxStatus.Pending;
                    msg.NextAttemptAt = now.AddSeconds(_options.RetryDelaySeconds);

                    _logger.LogWarning(
                        ex,
                        "Failed processing outbox message {OutboxId}. Attempts={Attempts}. Retrying at {NextAttemptAt}",
                        msg.Id,
                        msg.Attempts,
                        msg.NextAttemptAt
                    );
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static class OutboxStatus
    {
        public const string Pending = "PENDING";
        public const string Processing = "PROCESSING";
        public const string Published = "PUBLISHED";
        public const string Dead = "DEAD";
    }
}

// Options model to bind from config: "Outbox": { ... }
internal sealed class OutboxOptions
{
    public bool Enabled { get; init; } = true;
    public int PollingIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;

    public int MaxAttempts { get; init; } = 10;
    public int RetryDelaySeconds { get; init; } = 60;
}
