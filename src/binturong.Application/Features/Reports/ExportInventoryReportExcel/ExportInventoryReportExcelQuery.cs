using Application.Abstractions.Messaging;

namespace Application.Features.Reports.ExportInventoryReportExcel;

public sealed record ExportInventoryReportExcelQuery(Guid? CategoryId)
    : IQuery<ExportInventoryReportExcelResponse>;

public sealed record ExportInventoryReportExcelResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
