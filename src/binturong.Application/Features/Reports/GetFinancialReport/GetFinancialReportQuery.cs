using Application.Abstractions.Messaging;
using Application.ReadModels.Reports;

namespace Application.Features.Reports.GetFinancialReport;

public sealed record GetFinancialReportQuery(DateTime FromUtc, DateTime ToUtc)
    : IQuery<FinancialReportReadModel>;
