using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.History;

public sealed record ExportEmployeePaymentHistoryPdfQuery(
    Guid EmployeeId,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IQuery<byte[]>;
