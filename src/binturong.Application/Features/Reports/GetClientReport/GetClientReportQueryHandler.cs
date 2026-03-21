using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Application.ReadModels.Reports;
using Application.ReadModels.Sales;
using Application.ReadModels.Services;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Reports.GetClientReport;

internal sealed class GetClientReportQueryHandler
    : IQueryHandler<GetClientReportQuery, ClientReportReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetClientReportQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<ClientReportReadModel>> Handle(
        GetClientReportQuery q,
        CancellationToken ct
    )
    {
        var clients = _mongo.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var sales = _mongo.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);
        var services = _mongo.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);
        var invoices = _mongo.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var client = await clients.Find(x => x.ClientId == q.ClientId).FirstOrDefaultAsync(ct);
        if (client is null)
        {
            return Result.Failure<ClientReportReadModel>(
                Error.NotFound("Clients.NotFound", $"Client '{q.ClientId}' not found.")
            );
        }

        var clientSales = await sales
            .Find(x => x.ClientId == q.ClientId)
            .SortByDescending(x => x.OrderDate)
            .ToListAsync(ct);
        var clientServices = await services
            .Find(x => x.ClientId == q.ClientId)
            .SortByDescending(x => x.ScheduledDate)
            .ToListAsync(ct);
        var clientInvoices = await invoices
            .Find(x => x.ClientId == q.ClientId)
            .SortByDescending(x => x.IssueDate)
            .ToListAsync(ct);

        var hasData = clientSales.Any() || clientServices.Any() || clientInvoices.Any();

        return Result.Success(
            new ClientReportReadModel
            {
                ClientId = client.ClientId,
                ClientName = !string.IsNullOrWhiteSpace(client.TradeName)
                    ? client.TradeName
                    : client.ContactName,
                HasData = hasData,
                Message = hasData ? null : "No existen registros previos",
                Purchases = clientSales
                    .Select(x => new ClientPurchaseReportItemReadModel
                    {
                        SalesOrderId = x.SalesOrderId,
                        Code = x.Code,
                        OrderDate = x.OrderDate,
                        Total = x.Total,
                        Status = x.Status,
                    })
                    .ToArray(),
                Services = clientServices
                    .Select(x => new ClientServiceReportItemReadModel
                    {
                        ServiceOrderId = x.ServiceOrderId,
                        Code = x.Code,
                        ScheduledDate = x.ScheduledDate,
                        Status = x.Status,
                        ContractCode = x.ContractCode,
                    })
                    .ToArray(),
                Invoices = clientInvoices
                    .Select(x => new ClientInvoiceReportItemReadModel
                    {
                        InvoiceId = x.InvoiceId,
                        Consecutive = x.Consecutive,
                        IssueDate = x.IssueDate,
                        Total = x.Total,
                        PaidAmount = x.PaidAmount,
                        PendingAmount = x.PendingAmount,
                        TaxStatus = x.TaxStatus,
                    })
                    .ToArray(),
            }
        );
    }
}
