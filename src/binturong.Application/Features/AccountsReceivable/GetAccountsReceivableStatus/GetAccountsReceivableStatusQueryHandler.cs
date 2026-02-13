using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.AccountsReceivable.GetAccountsReceivableStatus;

internal sealed class GetAccountsReceivableStatusQueryHandler
    : IQueryHandler<GetAccountsReceivableStatusQuery, IReadOnlyList<InvoiceReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetAccountsReceivableStatusQueryHandler(
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
        GetAccountsReceivableStatusQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InvoiceReadModel>(MongoCollections.Invoices);

        var filter = BuildFilter(q);

        var page = q.Page <= 0 ? 1 : q.Page;
        var pageSize = q.PageSize <= 0 ? 50 : q.PageSize;

        var docs = await col.Find(filter)
            .SortByDescending(x => x.IssueDate)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "AccountsReceivable",
            "Invoice",
            null,
            "ACCOUNTS_RECEIVABLE_STATUS_READ",
            string.Empty,
            $"clientId={q.ClientId}; status={q.Status}; page={page}; pageSize={pageSize}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<InvoiceReadModel>>(docs);
    }

    private static FilterDefinition<InvoiceReadModel> BuildFilter(
        GetAccountsReceivableStatusQuery q
    )
    {
        var builder = Builders<InvoiceReadModel>.Filter;
        var filters = new List<FilterDefinition<InvoiceReadModel>>();

        if (q.ClientId.HasValue && q.ClientId.Value != Guid.Empty)
            filters.Add(builder.Eq(x => x.ClientId, q.ClientId.Value));

        if (!string.IsNullOrWhiteSpace(q.Status))
            filters.Add(builder.Eq(x => x.InternalStatus, q.Status));

        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }
}
