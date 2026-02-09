namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string SuppliersRead = "suppliers.read";
    public const string SuppliersCreate = "suppliers.create";
    public const string SuppliersUpdate = "suppliers.update";
    public const string SuppliersDelete = "suppliers.delete";

    public const string SuppliersCreditAssign = "suppliers.credit.assign";

    public const string SupplierQuotesRead = "supplier_quotes.read";
    public const string SupplierQuotesCreate = "supplier_quotes.create";
    public const string SupplierQuotesRespond = "supplier_quotes.respond";
    public const string SupplierQuotesReject = "supplier_quotes.reject";

    public const string SupplierEvaluationsRead = "supplier_evaluations.read";
    public const string SupplierEvaluationsCreate = "supplier_evaluations.create";
}
