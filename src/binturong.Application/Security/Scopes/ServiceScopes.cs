namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string ServicesRead = "services.read";
    public const string ServicesCreate = "services.create";
    public const string ServicesUpdate = "services.update";
    public const string ServicesDelete = "services.delete";

    public const string ServiceOrdersRead = "service_orders.read";
    public const string ServiceOrdersCreate = "service_orders.create";
    public const string ServiceOrdersUpdate = "service_orders.update";
    public const string ServiceOrdersAssignTechnician = "service_orders.assign_technician";
    public const string ServiceOrdersClose = "service_orders.close";

    public const string EmployeesWorkHistoryRead = "employees.work_history.read";
    public const string EmployeesWorkHistoryExport = "employees.work_history.export";
}
