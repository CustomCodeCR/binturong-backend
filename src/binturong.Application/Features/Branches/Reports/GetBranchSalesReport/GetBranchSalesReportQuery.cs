using Application.Abstractions.Messaging;
using Application.ReadModels.Reports;

namespace Application.Features.Branches.Reports.GetBranchSalesReport;

public sealed record GetBranchSalesReportQuery(
    Guid BranchId,
    DateTime? From,
    DateTime? To,
    string? Status
) : IQuery<BranchSalesReportReadModel>;
