using Application.Abstractions.Messaging;

namespace Application.Features.Reports.ExportFinancialReportPdf;

public sealed record ExportFinancialReportPdfQuery(DateTime FromUtc, DateTime ToUtc)
    : IQuery<ExportFinancialReportPdfResponse>;

public sealed record ExportFinancialReportPdfResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
