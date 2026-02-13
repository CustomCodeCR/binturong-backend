using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payments.GetPayments;

internal sealed class GetPaymentsQueryHandler
    : IQueryHandler<GetPaymentsQuery, IReadOnlyList<PaymentReadModel>>
{
    private readonly IMongoDatabase _mongo;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPaymentsQueryHandler(
        IMongoDatabase mongo,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _mongo = mongo;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<PaymentReadModel>>> Handle(
        GetPaymentsQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var filter = Builders<PaymentReadModel>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter = Builders<PaymentReadModel>.Filter.Or(
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.ClientName,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.Reference,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.PaymentMethodCode,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.PaymentDate)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "Payment",
            null,
            "PAYMENTS_LIST",
            string.Empty,
            $"page={q.Page}; pageSize={q.PageSize}; search={(q.Search ?? "")}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PaymentReadModel>>(docs);
    }
}
