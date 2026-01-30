using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Domain.Clients;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Clients.GetClientById;

internal sealed class GetClientByIdQueryHandler : IQueryHandler<GetClientByIdQuery, ClientReadModel>
{
    private readonly IMongoDatabase _db;

    public GetClientByIdQueryHandler(IMongoDatabase db) => _db = db;

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

        return Result.Success(doc);
    }
}
