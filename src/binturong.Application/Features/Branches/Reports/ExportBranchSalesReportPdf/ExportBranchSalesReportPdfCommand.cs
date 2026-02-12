using Application.Abstractions.Messaging;

namespace Application.Features.Branches.Reports.ExportBranchSalesReportPdf;

public sealed record ExportBranchSalesReportPdfCommand(
    Guid BranchId,
    DateTime? From,
    DateTime? To,
    string? Status
) : ICommand<byte[]>;
