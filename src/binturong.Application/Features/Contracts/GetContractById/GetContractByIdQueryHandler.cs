using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Contracts.GetContractById;

internal sealed class GetContractByIdQueryHandler
    : IQueryHandler<GetContractByIdQuery, ContractReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetContractByIdQueryHandler(
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

    public async Task<Result<ContractReadModel>> Handle(
        GetContractByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);

        var doc = await col.Find(x => x.ContractId == query.ContractId).FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Contracts",
            "Contract",
            query.ContractId,
            "CONTRACT_READ",
            string.Empty,
            $"contractId={query.ContractId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<ContractReadModel>(
                Error.NotFound("Contracts.NotFound", $"Contract '{query.ContractId}' not found")
            )
            : Result.Success(doc);
    }
}
