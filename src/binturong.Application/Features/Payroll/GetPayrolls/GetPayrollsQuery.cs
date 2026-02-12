using Application.Abstractions.Messaging;
using Application.ReadModels.Payroll;

namespace Application.Features.Payroll.GetPayrolls;

public sealed record GetPayrollsQuery(
    string? Search,
    DateTime? From,
    DateTime? To,
    string? Status,
    int Skip = 0,
    int Take = 50
) : IQuery<IReadOnlyList<PayrollReadModel>>;
