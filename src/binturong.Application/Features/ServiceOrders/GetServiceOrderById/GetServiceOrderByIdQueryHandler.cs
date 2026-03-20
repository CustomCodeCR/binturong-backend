using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Services;
using Domain.ServiceOrders;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.ServiceOrders.GetServiceOrderById;

internal sealed class GetServiceOrderByIdQueryHandler
    : IQueryHandler<GetServiceOrderByIdQuery, ServiceOrderReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetServiceOrderByIdQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<ServiceOrderReadModel>> Handle(
        GetServiceOrderByIdQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var doc = await col.Find(x => x.Id == $"service_order:{q.ServiceOrderId}")
            .FirstOrDefaultAsync(ct);

        return doc is null
            ? Result.Failure<ServiceOrderReadModel>(ServiceOrderErrors.NotFound(q.ServiceOrderId))
            : Result.Success(doc);
    }
}
