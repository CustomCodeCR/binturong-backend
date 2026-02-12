using Application.Abstractions.Messaging;
using Application.ReadModels.Payroll;

namespace Application.Features.Payroll.GetPayrollById;

public sealed record GetPayrollByIdQuery(Guid PayrollId) : IQuery<PayrollReadModel>;
