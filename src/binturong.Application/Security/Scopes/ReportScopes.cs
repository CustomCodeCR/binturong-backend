namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string ReportsFinancialRead = "reports.financial.read";
    public const string ReportsFinancialExportPdf = "reports.financial.export_pdf";

    public const string ReportsInventoryRead = "reports.inventory.read";
    public const string ReportsInventoryExportExcel = "reports.inventory.export_excel";

    public const string ReportsClientsRead = "reports.clients.read";
    public const string ReportsClientsExport = "reports.clients.export";

    public const string ReportsServiceOrdersRead = "reports.service_orders.read";
    public const string ReportsServiceOrdersExport = "reports.service_orders.export";

    public const string ReportsSchedulesRead = "reports.schedules.read";
    public const string ReportsSchedulesCreate = "reports.schedules.create";
    public const string ReportsSchedulesUpdate = "reports.schedules.update";
}
