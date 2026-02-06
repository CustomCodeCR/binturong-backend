using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Domain.Clients;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Clients.GetClientById;

internal sealed class GetClientByIdQueryHandler : IQueryHandler<GetClientByIdQuery, ClientReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetClientByIdQueryHandler(
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

    public async Task<Result<ClientReadModel>> Handle(
        GetClientByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{query.ClientId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<ClientReadModel>(ClientErrors.NotFound(query.ClientId));

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Clients",
            "Client",
            query.ClientId,
            "CLIENT_READ",
            string.Empty,
            $"clientId={query.ClientId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
