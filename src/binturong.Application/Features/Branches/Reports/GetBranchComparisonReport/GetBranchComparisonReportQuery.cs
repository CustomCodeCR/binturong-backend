using Application.Abstractions.Messaging;
using Application.ReadModels.Reports;

namespace Application.Features.Branches.Reports.GetBranchComparisonReport;

public sealed record GetBranchComparisonReportQuery(
    Guid BranchAId,
    Guid BranchBId,
    DateTime? From,
    DateTime? To,
    string? Status
) : IQuery<BranchComparisonReportReadModel>;
