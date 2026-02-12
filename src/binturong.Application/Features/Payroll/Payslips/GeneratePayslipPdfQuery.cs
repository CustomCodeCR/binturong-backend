using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Payslips;

public sealed record GeneratePayslipPdfQuery(Guid PayrollId, Guid EmployeeId) : IQuery<byte[]>;
