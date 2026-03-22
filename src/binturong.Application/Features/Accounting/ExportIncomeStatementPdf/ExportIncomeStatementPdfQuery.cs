using Application.Abstractions.Messaging;

namespace Application.Features.Accounting.ExportIncomeStatementPdf;

public sealed record ExportIncomeStatementPdfQuery(DateTime FromUtc, DateTime ToUtc)
    : IQuery<ExportIncomeStatementPdfResponse>;

public sealed record ExportIncomeStatementPdfResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
