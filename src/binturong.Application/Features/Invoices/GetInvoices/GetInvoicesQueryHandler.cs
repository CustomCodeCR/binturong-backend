using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Invoices.GetInvoices;

internal sealed class GetInvoicesQueryHandler
    : IQueryHandler<GetInvoicesQuery, IReadOnlyList<InvoiceReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetInvoicesQueryHandler(
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

    public async Task<Result<IReadOnlyList<InvoiceReadModel>>> Handle(
        GetInvoicesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.IssueDate)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Invoices",
            "Invoice",
            null,
            "INVOICE_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<InvoiceReadModel>>(docs);
    }

    private static FilterDefinition<InvoiceReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<InvoiceReadModel>.Filter.Empty;

        var s = search.Trim();

        return Builders<InvoiceReadModel>.Filter.Or(
            Builders<InvoiceReadModel>.Filter.Regex(
                x => x.Consecutive,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<InvoiceReadModel>.Filter.Regex(
                x => x.TaxKey,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<InvoiceReadModel>.Filter.Regex(
                x => x.ClientName,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            )
        );
    }
}
