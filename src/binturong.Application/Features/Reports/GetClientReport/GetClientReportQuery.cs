using Application.Abstractions.Messaging;
using Application.ReadModels.Reports;

namespace Application.Features.Reports.GetClientReport;

public sealed record GetClientReportQuery(Guid ClientId) : IQuery<ClientReportReadModel>;
