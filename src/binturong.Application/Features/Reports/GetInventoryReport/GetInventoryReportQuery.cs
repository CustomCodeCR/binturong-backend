using Application.Abstractions.Messaging;
using Application.ReadModels.Reports;

namespace Application.Features.Reports.GetInventoryReport;

public sealed record GetInventoryReportQuery(Guid? CategoryId) : IQuery<InventoryReportReadModel>;
