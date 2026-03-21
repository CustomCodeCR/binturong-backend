using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Reports;
using Application.ReadModels.Services;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Reports.GetServiceOrdersReport;

internal sealed class GetServiceOrdersReportQueryHandler
    : IQueryHandler<GetServiceOrdersReportQuery, ServiceOrdersReportReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetServiceOrdersReportQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<ServiceOrdersReportReadModel>> Handle(
        GetServiceOrdersReportQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var docs = await col.Find(x => x.ScheduledDate >= q.FromUtc && x.ScheduledDate <= q.ToUtc)
            .ToListAsync(ct);

        if (q.EmployeeId.HasValue)
        {
            docs = docs.Where(x => x.Technicians.Any(t => t.EmployeeId == q.EmployeeId.Value))
                .ToList();
        }

        if (docs.Count == 0)
        {
            return Result.Success(
                new ServiceOrdersReportReadModel
                {
                    FromUtc = q.FromUtc,
                    ToUtc = q.ToUtc,
                    CompletedCount = 0,
                    PendingCount = 0,
                    CanceledCount = 0,
                    HasData = false,
                    Message = "Sin información disponible",
                    Items = [],
                }
            );
        }

        return Result.Success(
            new ServiceOrdersReportReadModel
            {
                FromUtc = q.FromUtc,
                ToUtc = q.ToUtc,
                CompletedCount = docs.Count(x =>
                    string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.Status, "Closed", StringComparison.OrdinalIgnoreCase)
                ),
                PendingCount = docs.Count(x =>
                    string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase)
                ),
                CanceledCount = docs.Count(x =>
                    string.Equals(x.Status, "Canceled", StringComparison.OrdinalIgnoreCase)
                ),
                HasData = true,
                Message = null,
                Items = docs.OrderByDescending(x => x.ScheduledDate)
                    .Select(x => new ServiceOrdersReportItemReadModel
                    {
                        ServiceOrderId = x.ServiceOrderId,
                        Code = x.Code,
                        ClientName = x.ClientName,
                        ScheduledDate = x.ScheduledDate,
                        Status = x.Status,
                        ServiceAddress = x.ServiceAddress,
                        Technicians = x.Technicians.Select(t => t.EmployeeName).ToArray(),
                        Services = x.Services.Select(s => s.ServiceName).ToArray(),
                    })
                    .ToArray(),
            }
        );
    }
}
