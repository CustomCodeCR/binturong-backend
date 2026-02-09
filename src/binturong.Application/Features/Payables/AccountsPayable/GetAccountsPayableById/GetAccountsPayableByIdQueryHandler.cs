using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Payables;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payables.AccountsPayable.GetAccountsPayableById;

internal sealed class GetAccountsPayableByIdQueryHandler
    : IQueryHandler<GetAccountsPayableByIdQuery, AccountsPayableReadModel>
{
    private const string CollectionName = "accounts_payable";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetAccountsPayableByIdQueryHandler(
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

    public async Task<Result<AccountsPayableReadModel>> Handle(
        GetAccountsPayableByIdQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<AccountsPayableReadModel>(CollectionName);

        var doc = await col.Find(x => x.AccountPayableId == q.AccountPayableId)
            .FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payables",
            "AccountsPayable",
            q.AccountPayableId,
            "ACCOUNTS_PAYABLE_READ",
            string.Empty,
            $"accountPayableId={q.AccountPayableId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<AccountsPayableReadModel>(
                Error.NotFound(
                    "AccountsPayable.NotFound",
                    $"Account payable '{q.AccountPayableId}' not found"
                )
            )
            : Result.Success(doc);
    }
}
