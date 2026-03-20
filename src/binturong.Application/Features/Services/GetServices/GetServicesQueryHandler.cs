using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Services;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Services.GetServices;

internal sealed class GetServicesQueryHandler
    : IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceReadModel>>
{
    private readonly IMongoDatabase _mongo;

    public GetServicesQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IReadOnlyList<ServiceReadModel>>> Handle(
        GetServicesQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<ServiceReadModel>(MongoCollections.Services);

        var builder = Builders<ServiceReadModel>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter &= builder.Or(
                builder.Regex(x => x.Code, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.CategoryName, new MongoDB.Bson.BsonRegularExpression(s, "i"))
            );
        }

        if (q.CategoryId.HasValue)
            filter &= builder.Eq(x => x.CategoryId, q.CategoryId.Value);

        if (q.IsActive.HasValue)
            filter &= builder.Eq(x => x.IsActive, q.IsActive.Value);

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortBy(x => x.CategoryName)
            .ThenBy(x => x.Name)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<ServiceReadModel>>(docs);
    }
}
