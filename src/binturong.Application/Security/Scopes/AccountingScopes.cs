namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string AccountingRead = "accounting.read";
    public const string AccountingCreateIncome = "accounting.create_income";
    public const string AccountingCreateExpense = "accounting.create_expense";
    public const string AccountingReconcile = "accounting.reconcile";

    public const string AccountingIncomeStatementRead = "accounting.income_statement.read";
    public const string AccountingIncomeStatementExport = "accounting.income_statement.export";

    public const string AccountingCashFlowRead = "accounting.cash_flow.read";
    public const string AccountingCashFlowExport = "accounting.cash_flow.export";
}
