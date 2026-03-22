using Application.Abstractions.Messaging;

namespace Application.Features.Accounting.ExportCashFlowExcel;

public sealed record ExportCashFlowExcelQuery(DateTime FromUtc, DateTime ToUtc)
    : IQuery<ExportCashFlowExcelResponse>;

public sealed record ExportCashFlowExcelResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
