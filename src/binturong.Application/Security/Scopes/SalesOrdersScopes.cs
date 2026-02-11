namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string SalesOrdersRead = "sales_orders.read";
    public const string SalesOrdersCreate = "sales_orders.create";
    public const string SalesOrdersConvertFromQuote = "sales_orders.convert_from_quote";
    public const string SalesOrdersConfirm = "sales_orders.confirm";
}
