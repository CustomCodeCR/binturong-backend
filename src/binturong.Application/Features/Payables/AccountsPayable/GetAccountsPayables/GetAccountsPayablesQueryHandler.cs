using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Payables;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payables.AccountsPayable.GetAccountsPayables;

internal sealed class GetAccountsPayablesQueryHandler
    : IQueryHandler<GetAccountsPayablesQuery, IReadOnlyList<AccountsPayableReadModel>>
{
    private const string CollectionName = "accounts_payable";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetAccountsPayablesQueryHandler(
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

    public async Task<Result<IReadOnlyList<AccountsPayableReadModel>>> Handle(
        GetAccountsPayablesQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<AccountsPayableReadModel>(CollectionName);
        var filter = BuildFilter(q.Search, q.OnlyOverdue);

        var docs = await col.Find(filter)
            .SortBy(x => x.DueDate)
            .Skip(q.Skip)
            .Limit(q.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payables",
            "AccountsPayable",
            null,
            "ACCOUNTS_PAYABLE_LIST_READ",
            string.Empty,
            $"search={q.Search}; onlyOverdue={q.OnlyOverdue}; skip={q.Skip}; take={q.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<AccountsPayableReadModel>>(docs);
    }

    private static FilterDefinition<AccountsPayableReadModel> BuildFilter(
        string? search,
        bool? onlyOverdue
    )
    {
        var f = Builders<AccountsPayableReadModel>.Filter;
        var filter = f.Empty;

        if (onlyOverdue == true)
        {
            var now = DateTime.UtcNow;
            filter &= f.And(f.Lt(x => x.DueDate, now), f.Gt(x => x.PendingBalance, 0));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var rx = new BsonRegularExpression(s, "i");

            filter &= f.Or(
                f.Regex(x => x.SupplierName, rx),
                f.Regex(x => x.Status, rx),
                f.Regex(x => x.Currency, rx),
                f.Regex(x => x.SupplierInvoiceId, rx)
            );
        }

        return filter;
    }
}
