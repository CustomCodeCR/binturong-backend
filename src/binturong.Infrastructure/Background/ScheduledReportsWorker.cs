using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Features.Reports.GetClientReport;
using Application.Features.Reports.GetFinancialReport;
using Application.Features.Reports.GetInventoryReport;
using Application.Features.Reports.GetServiceOrdersReport;
using Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Background;

public sealed class ScheduledReportsWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduledReportsWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

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
        var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var financial = scope.ServiceProvider.GetRequiredService<
            IQueryHandler<
                GetFinancialReportQuery,
                Application.ReadModels.Reports.FinancialReportReadModel
            >
        >();
        var inventory = scope.ServiceProvider.GetRequiredService<
            IQueryHandler<
                GetInventoryReportQuery,
                Application.ReadModels.Reports.InventoryReportReadModel
            >
        >();
        var client = scope.ServiceProvider.GetRequiredService<
            IQueryHandler<
                GetClientReportQuery,
                Application.ReadModels.Reports.ClientReportReadModel
            >
        >();
        var serviceOrders = scope.ServiceProvider.GetRequiredService<
            IQueryHandler<
                GetServiceOrdersReportQuery,
                Application.ReadModels.Reports.ServiceOrdersReportReadModel
            >
        >();

        var nowUtc = DateTime.UtcNow;

        var schedules = await db.ReportSchedules.Where(x => x.IsActive).ToListAsync(ct);

        foreach (var schedule in schedules)
        {
            if (!schedule.ShouldRun(nowUtc))
                continue;

            try
            {
                var html = await BuildHtmlAsync(
                    schedule,
                    nowUtc,
                    financial,
                    inventory,
                    client,
                    serviceOrders,
                    ct
                );

                await email.SendAsync(
                    schedule.RecipientEmail,
                    $"Scheduled report: {schedule.Name}",
                    html,
                    ct
                );

                schedule.MarkExecutionSucceeded(nowUtc);
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                schedule.MarkExecutionFailed(ex.Message, nowUtc);
                await db.SaveChangesAsync(ct);
            }
        }
    }

    private static async Task<string> BuildHtmlAsync(
        ReportSchedule schedule,
        DateTime nowUtc,
        IQueryHandler<
            GetFinancialReportQuery,
            Application.ReadModels.Reports.FinancialReportReadModel
        > financial,
        IQueryHandler<
            GetInventoryReportQuery,
            Application.ReadModels.Reports.InventoryReportReadModel
        > inventory,
        IQueryHandler<
            GetClientReportQuery,
            Application.ReadModels.Reports.ClientReportReadModel
        > client,
        IQueryHandler<
            GetServiceOrdersReportQuery,
            Application.ReadModels.Reports.ServiceOrdersReportReadModel
        > serviceOrders,
        CancellationToken ct
    )
    {
        if (string.Equals(schedule.ReportType, "Financial", StringComparison.OrdinalIgnoreCase))
        {
            var fromUtc = nowUtc.Date.AddDays(-30);
            var result = await financial.Handle(new GetFinancialReportQuery(fromUtc, nowUtc), ct);
            var report = result.Value;

            return $"""
                <h1>Financial Report</h1>
                <p>Sales: {report.SalesTotal:N2}</p>
                <p>Expenses: {report.ExpensesTotal:N2}</p>
                <p>Profit: {report.Profit:N2}</p>
                """;
        }

        if (string.Equals(schedule.ReportType, "Inventory", StringComparison.OrdinalIgnoreCase))
        {
            var result = await inventory.Handle(
                new GetInventoryReportQuery(schedule.CategoryId),
                ct
            );
            var report = result.Value;

            var rows = string.Join(
                "",
                report.Items.Select(x =>
                    $"<tr><td>{x.ProductName}</td><td>{x.CategoryName}</td><td>{x.TotalStock:N2}</td></tr>"
                )
            );

            return $"""
                <h1>Inventory Report</h1>
                <table border="1" cellspacing="0" cellpadding="6">
                    <tr><th>Product</th><th>Category</th><th>Total Stock</th></tr>
                    {rows}
                </table>
                """;
        }

        if (string.Equals(schedule.ReportType, "ClientHistory", StringComparison.OrdinalIgnoreCase))
        {
            if (!schedule.ClientId.HasValue)
                return "<p>ClientId is required for ClientHistory report.</p>";

            var result = await client.Handle(new GetClientReportQuery(schedule.ClientId.Value), ct);
            var report = result.Value;

            return $"""
                <h1>Client Report</h1>
                <p>Client: {report.ClientName}</p>
                <p>Purchases: {report.Purchases.Count}</p>
                <p>Services: {report.Services.Count}</p>
                <p>Invoices: {report.Invoices.Count}</p>
                """;
        }

        if (string.Equals(schedule.ReportType, "ServiceOrders", StringComparison.OrdinalIgnoreCase))
        {
            var fromUtc = nowUtc.Date.AddDays(-30);
            var result = await serviceOrders.Handle(
                new GetServiceOrdersReportQuery(fromUtc, nowUtc, schedule.EmployeeId),
                ct
            );
            var report = result.Value;

            return $"""
                <h1>Service Orders Report</h1>
                <p>Completed: {report.CompletedCount}</p>
                <p>Pending: {report.PendingCount}</p>
                <p>Canceled: {report.CanceledCount}</p>
                """;
        }

        return "<p>Unsupported report type.</p>";
    }
}
