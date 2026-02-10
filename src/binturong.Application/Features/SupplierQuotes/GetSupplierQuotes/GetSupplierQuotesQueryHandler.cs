using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.SupplierQuotes.GetSupplierQuotes;

internal sealed class GetSupplierQuotesQueryHandler
    : IQueryHandler<GetSupplierQuotesQuery, IReadOnlyList<SupplierQuoteReadModel>>
{
    private const string CollectionName = "supplier_quotes";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSupplierQuotesQueryHandler(
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

    public async Task<Result<IReadOnlyList<SupplierQuoteReadModel>>> Handle(
        GetSupplierQuotesQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierQuoteReadModel>(CollectionName);

        var filter = BuildFilter(q);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.RequestedAtUtc)
            .Skip(q.Skip)
            .Limit(q.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Suppliers",
            "SupplierQuote",
            null,
            "SUPPLIER_QUOTE_LIST_READ",
            string.Empty,
            $"search={q.Search}; supplierId={q.SupplierId}; branchId={q.BranchId}; status={q.Status}; skip={q.Skip}; take={q.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<SupplierQuoteReadModel>>(docs);
    }

    private static FilterDefinition<SupplierQuoteReadModel> BuildFilter(GetSupplierQuotesQuery q)
    {
        var f = Builders<SupplierQuoteReadModel>.Filter.Empty;

        if (q.SupplierId.HasValue && q.SupplierId.Value != Guid.Empty)
            f &= Builders<SupplierQuoteReadModel>.Filter.Eq(x => x.SupplierId, q.SupplierId.Value);

        if (q.BranchId.HasValue && q.BranchId.Value != Guid.Empty)
            f &= Builders<SupplierQuoteReadModel>.Filter.Eq(x => x.BranchId, q.BranchId.Value);

        if (!string.IsNullOrWhiteSpace(q.Status))
            f &= Builders<SupplierQuoteReadModel>.Filter.Eq(x => x.Status, q.Status.Trim());

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            var rx = new BsonRegularExpression(s, "i");

            var searchFilter = Builders<SupplierQuoteReadModel>.Filter.Or(
                Builders<SupplierQuoteReadModel>.Filter.Regex(x => x.Code, rx),
                Builders<SupplierQuoteReadModel>.Filter.Regex(x => x.SupplierName, rx),
                Builders<SupplierQuoteReadModel>.Filter.Regex(x => x.BranchName, rx),
                Builders<SupplierQuoteReadModel>.Filter.Regex(x => x.Status, rx),
                Builders<SupplierQuoteReadModel>.Filter.Regex(x => x.Notes, rx),
                Builders<SupplierQuoteReadModel>.Filter.Regex(x => x.SupplierMessage, rx),
                Builders<SupplierQuoteReadModel>.Filter.Regex(x => x.RejectReason, rx)
            );

            f &= searchFilter;
        }

        return f;
    }
}
