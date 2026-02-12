using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.History;

public sealed record ExportEmployeePaymentHistoryExcelQuery(
    Guid EmployeeId,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IQuery<byte[]>;
