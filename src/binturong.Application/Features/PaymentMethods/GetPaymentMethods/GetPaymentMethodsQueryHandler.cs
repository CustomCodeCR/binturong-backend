using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.MasterData;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.PaymentMethods.GetPaymentMethods;

internal sealed class GetPaymentMethodsQueryHandler
    : IQueryHandler<GetPaymentMethodsQuery, IReadOnlyList<PaymentMethodReadModel>>
{
    private const string CollectionName = "payment_methods";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPaymentMethodsQueryHandler(
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

    public async Task<Result<IReadOnlyList<PaymentMethodReadModel>>> Handle(
        GetPaymentMethodsQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PaymentMethodReadModel>(CollectionName);

        var filter = BuildFilter(q.Search);

        var page = q.Page <= 0 ? 1 : q.Page;
        var pageSize = q.PageSize <= 0 ? 50 : q.PageSize;

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "PaymentMethods",
            "PaymentMethod",
            null,
            "PAYMENT_METHOD_LIST_READ",
            string.Empty,
            $"search={q.Search}; page={page}; pageSize={pageSize}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PaymentMethodReadModel>>(docs);
    }

    private static FilterDefinition<PaymentMethodReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<PaymentMethodReadModel>.Filter.Empty;

        var s = search.Trim();

        // match Code or Description (case-insensitive)
        return Builders<PaymentMethodReadModel>.Filter.Or(
            Builders<PaymentMethodReadModel>.Filter.Regex(
                x => x.Code,
                new BsonRegularExpression(s, "i")
            ),
            Builders<PaymentMethodReadModel>.Filter.Regex(
                x => x.Description,
                new BsonRegularExpression(s, "i")
            )
        );
    }
}
