using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Invoices.GetInvoiceById;

internal sealed class GetInvoiceByIdQueryHandler
    : IQueryHandler<GetInvoiceByIdQuery, InvoiceReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetInvoiceByIdQueryHandler(
        IMongoDatabase db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<InvoiceReadModel>> Handle(
        GetInvoiceByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var doc = await col.Find(x => x.Id == $"invoice:{query.Id}").FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Invoices",
            "Invoice",
            query.Id,
            "INVOICE_READ",
            string.Empty,
            doc is null ? "not_found" : "ok",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<InvoiceReadModel>(
                Error.NotFound("Invoices.NotFound", $"Invoice '{query.Id}' not found.")
            )
            : Result.Success(doc);
    }
}
