using Application.Abstractions.Messaging;

namespace Application.Features.Branches.Reports.ExportBranchSalesReportExcel;

public sealed record ExportBranchSalesReportExcelCommand(
    Guid BranchId,
    DateTime? From,
    DateTime? To,
    string? Status
) : ICommand<byte[]>;
