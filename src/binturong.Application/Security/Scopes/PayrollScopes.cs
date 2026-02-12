namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string PayrollRead = "payroll.read";
    public const string PayrollCreate = "payroll.create";
    public const string PayrollUpdate = "payroll.update";
    public const string PayrollOvertimeManage = "payroll.overtime.manage";
    public const string PayrollCommissionManage = "payroll.commission.manage";
    public const string PayrollPayslipRead = "payroll.payslip.read";
    public const string PayrollPayslipSend = "payroll.payslip.send";
    public const string PayrollExport = "payroll.export";
}
