using Application.Abstractions.Messaging;

namespace Application.Features.Reports.ExportServiceOrdersReportExcel;

public sealed record ExportServiceOrdersReportExcelQuery(
    DateTime FromUtc,
    DateTime ToUtc,
    Guid? EmployeeId
) : IQuery<ExportServiceOrdersReportExcelResponse>;

public sealed record ExportServiceOrdersReportExcelResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
