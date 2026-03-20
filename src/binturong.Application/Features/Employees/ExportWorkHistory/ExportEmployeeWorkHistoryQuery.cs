using Application.Abstractions.Messaging;

namespace Application.Features.Employees.ExportWorkHistory;

public sealed record ExportEmployeeWorkHistoryQuery(Guid EmployeeId)
    : IQuery<ExportEmployeeWorkHistoryResponse>;

public sealed record ExportEmployeeWorkHistoryResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
