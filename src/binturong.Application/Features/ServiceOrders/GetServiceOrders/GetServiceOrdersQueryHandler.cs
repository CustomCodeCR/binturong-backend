using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Services;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.ServiceOrders.GetServiceOrders;

internal sealed class GetServiceOrdersQueryHandler
    : IQueryHandler<GetServiceOrdersQuery, IReadOnlyList<ServiceOrderReadModel>>
{
    private readonly IMongoDatabase _mongo;

    public GetServiceOrdersQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IReadOnlyList<ServiceOrderReadModel>>> Handle(
        GetServiceOrdersQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var builder = Builders<ServiceOrderReadModel>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter &= builder.Or(
                builder.Regex(x => x.Code, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.ClientName, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.ServiceAddress, new MongoDB.Bson.BsonRegularExpression(s, "i"))
            );
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            filter &= builder.Eq(x => x.Status, q.Status.Trim());

        if (q.ContractId.HasValue)
            filter &= builder.Eq(x => x.ContractId, q.ContractId.Value);

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.ScheduledDate)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<ServiceOrderReadModel>>(docs);
    }
}
