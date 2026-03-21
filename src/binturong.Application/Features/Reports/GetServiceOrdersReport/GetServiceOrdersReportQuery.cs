using Application.Abstractions.Messaging;
using Application.ReadModels.Reports;

namespace Application.Features.Reports.GetServiceOrdersReport;

public sealed record GetServiceOrdersReportQuery(DateTime FromUtc, DateTime ToUtc, Guid? EmployeeId)
    : IQuery<ServiceOrdersReportReadModel>;
