using Application.Abstractions.Messaging;
using Application.ReadModels.Reports;

namespace Application.Features.Reports.GetSchedules;

public sealed record GetReportSchedulesQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<ReportScheduleReadModel>>;
