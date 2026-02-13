using Application.Abstractions.Background;
using Application.Abstractions.Data;
using Application.Abstractions.Notifications;
using Application.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Background;

public sealed class ContractRenewalWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EmailOptions _emailOptions;

    public ContractRenewalWorker(IServiceScopeFactory scopeFactory, EmailOptions emailOptions)
    {
        _scopeFactory = scopeFactory;
        _emailOptions = emailOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch { }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var realtime = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
        var jobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobScheduler>();

        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(nowUtc);

        var contracts = await db
            .Contracts.Where(c => c.EndDate != null && c.Status != "Deleted")
            .ToListAsync(ct);

        foreach (var c in contracts)
        {
            if (c.IsExpiringSoon(today) && !c.ExpiryAlertActive)
            {
                c.MarkExpiryNotified(nowUtc);

                await realtime.NotifyRoleAsync(
                    "ContractsManager",
                    "contracts.expiry_soon",
                    new
                    {
                        contractId = c.Id,
                        clientId = c.ClientId,
                        endDate = c.EndDate,
                        noticeDays = c.ExpiryNoticeDays,
                        atUtc = nowUtc,
                    },
                    ct
                );

                var to =
                    _emailOptions.ContractsEmail ?? _emailOptions.AdminEmail ?? "contracts@local";

                await jobs.EnqueueAsync(
                    async (sp, token) =>
                    {
                        var email = sp.GetRequiredService<IEmailSender>();
                        await email.SendAsync(
                            to,
                            "Alerta: contrato por vencer",
                            $@"
<h3>Contrato por vencer</h3>
<p><b>ContractId:</b> {c.Id}</p>
<p><b>ClientId:</b> {c.ClientId}</p>
<p><b>EndDate:</b> {c.EndDate}</p>
<p><b>At:</b> {nowUtc:O}</p>
",
                            token
                        );
                    },
                    ct
                );
            }

            if (c.AutoRenewEnabled && c.IsExpired(today))
            {
                c.Renew(today, nowUtc);

                await realtime.NotifyRoleAsync(
                    "ContractsManager",
                    "contracts.renewed",
                    new
                    {
                        contractId = c.Id,
                        clientId = c.ClientId,
                        newStart = c.StartDate,
                        newEnd = c.EndDate,
                        atUtc = nowUtc,
                    },
                    ct
                );

                var to =
                    _emailOptions.ContractsEmail ?? _emailOptions.AdminEmail ?? "contracts@local";

                await jobs.EnqueueAsync(
                    async (sp, token) =>
                    {
                        var email = sp.GetRequiredService<IEmailSender>();
                        await email.SendAsync(
                            to,
                            "Contrato renovado automáticamente",
                            $@"
<h3>Renovación automática</h3>
<p><b>ContractId:</b> {c.Id}</p>
<p><b>ClientId:</b> {c.ClientId}</p>
<p><b>New Start:</b> {c.StartDate}</p>
<p><b>New End:</b> {c.EndDate}</p>
<p><b>At:</b> {nowUtc:O}</p>
",
                            token
                        );
                    },
                    ct
                );
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
