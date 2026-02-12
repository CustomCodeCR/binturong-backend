using Application.Abstractions.Background;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Options;
using Domain.WarehouseStocks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Inventory.Alerts;

internal static class LowStockAlertHelper
{
    private static readonly TimeSpan EmailThrottle = TimeSpan.FromMinutes(30);

    public static async Task HandleLowStockAsync(
        WarehouseStock stock,
        Guid productId,
        Guid warehouseId,
        DateTime nowUtc,
        IRealtimeNotifier realtime,
        IBackgroundJobScheduler jobs,
        EmailOptions emailOptions,
        ICurrentUser currentUser,
        CancellationToken ct
    )
    {
        var isLow = stock.IsLowStock();

        if (isLow && !stock.LowStockAlertActive)
        {
            stock.MarkLowStockNotified(nowUtc);

            await realtime.NotifyRoleAsync(
                "Purchases",
                "inventory.low_stock",
                new
                {
                    productId,
                    warehouseId,
                    currentStock = stock.CurrentStock,
                    minStock = stock.MinStock,
                    triggeredAtUtc = nowUtc,
                    triggeredByUserId = currentUser.UserId,
                },
                ct
            );

            var to = emailOptions.PurchasesEmail ?? "purchases@local";
            await jobs.EnqueueAsync(
                async (sp, token) =>
                {
                    var email = sp.GetRequiredService<IEmailSender>();
                    await email.SendAsync(
                        to,
                        "Alerta: Inventario bajo",
                        $@"
<h3>Inventario bajo</h3>
<p><b>ProductId:</b> {productId}</p>
<p><b>WarehouseId:</b> {warehouseId}</p>
<p><b>Current:</b> {stock.CurrentStock}</p>
<p><b>Min:</b> {stock.MinStock}</p>
<p><b>At:</b> {nowUtc:O}</p>
",
                        token
                    );
                },
                ct
            );

            return;
        }

        if (!isLow && stock.LowStockAlertActive)
        {
            stock.ClearLowStockAlert();

            await realtime.NotifyRoleAsync(
                "Purchases",
                "inventory.low_stock_cleared",
                new
                {
                    productId,
                    warehouseId,
                    currentStock = stock.CurrentStock,
                    minStock = stock.MinStock,
                    clearedAtUtc = nowUtc,
                    clearedByUserId = currentUser.UserId,
                },
                ct
            );

            return;
        }

        if (isLow && stock.LowStockAlertActive)
        {
            var last = stock.LowStockLastNotifiedAtUtc;
            if (last is null || (nowUtc - last.Value) >= EmailThrottle)
            {
                stock.MarkLowStockNotified(nowUtc);

                var to = emailOptions.PurchasesEmail ?? "purchases@local";
                await jobs.EnqueueAsync(
                    async (sp, token) =>
                    {
                        var email = sp.GetRequiredService<IEmailSender>();
                        await email.SendAsync(
                            to,
                            "Recordatorio: Inventario sigue bajo",
                            $@"
<h3>Inventario aún bajo</h3>
<p><b>ProductId:</b> {productId}</p>
<p><b>WarehouseId:</b> {warehouseId}</p>
<p><b>Current:</b> {stock.CurrentStock}</p>
<p><b>Min:</b> {stock.MinStock}</p>
<p><b>At:</b> {nowUtc:O}</p>
",
                            token
                        );
                    },
                    ct
                );
            }
        }
    }
}
