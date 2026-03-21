using Application.Abstractions.Messaging;

namespace Application.Features.Reports.ExportClientReportExcel;

public sealed record ExportClientReportExcelQuery(Guid ClientId)
    : IQuery<ExportClientReportExcelResponse>;

public sealed record ExportClientReportExcelResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
