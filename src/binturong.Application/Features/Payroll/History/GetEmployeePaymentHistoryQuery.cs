using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.History;

public sealed record GetEmployeePaymentHistoryQuery(
    Guid EmployeeId,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IQuery<IReadOnlyList<EmployeePaymentHistoryRow>>;
