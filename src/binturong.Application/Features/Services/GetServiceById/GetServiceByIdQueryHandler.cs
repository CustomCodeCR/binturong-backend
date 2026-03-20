using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Services;
using Domain.Services;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Services.GetServiceById;

internal sealed class GetServiceByIdQueryHandler
    : IQueryHandler<GetServiceByIdQuery, ServiceReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetServiceByIdQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<ServiceReadModel>> Handle(GetServiceByIdQuery q, CancellationToken ct)
    {
        var col = _mongo.GetCollection<ServiceReadModel>(MongoCollections.Services);

        var doc = await col.Find(x => x.Id == $"service:{q.ServiceId}").FirstOrDefaultAsync(ct);
        return doc is null
            ? Result.Failure<ServiceReadModel>(ServiceErrors.NotFound(q.ServiceId))
            : Result.Success(doc);
    }
}
