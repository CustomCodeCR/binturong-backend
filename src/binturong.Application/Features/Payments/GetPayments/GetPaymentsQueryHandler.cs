using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payments.GetPayments;

internal sealed class GetPaymentsQueryHandler
    : IQueryHandler<GetPaymentsQuery, IReadOnlyList<PaymentReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPaymentsQueryHandler(
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

    public async Task<Result<IReadOnlyList<PaymentReadModel>>> Handle(
        GetPaymentsQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var filter = BuildFilter(q);

        var page = q.Page <= 0 ? 1 : q.Page;
        var pageSize = q.PageSize <= 0 ? 50 : q.PageSize;

        var docs = await col.Find(filter)
            .SortByDescending(x => x.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "Payment",
            null,
            "PAYMENTS_LIST_READ",
            string.Empty,
            $"search={q.Search}; paymentMethodId={q.PaymentMethodId}; page={page}; pageSize={pageSize}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PaymentReadModel>>(docs);
    }

    private static FilterDefinition<PaymentReadModel> BuildFilter(GetPaymentsQuery q)
    {
        var builder = Builders<PaymentReadModel>.Filter;
        var filters = new List<FilterDefinition<PaymentReadModel>>();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();

            var searchFilter = builder.Or(
                builder.Regex(x => x.Reference, new BsonRegularExpression(s, "i")),
                builder.Regex(x => x.Notes, new BsonRegularExpression(s, "i")),
                builder.Regex(x => x.ClientName, new BsonRegularExpression(s, "i")),
                builder.Regex(x => x.PaymentMethodCode, new BsonRegularExpression(s, "i"))
            );

            filters.Add(searchFilter);
        }

        if (q.PaymentMethodId.HasValue && q.PaymentMethodId.Value != Guid.Empty)
            filters.Add(builder.Eq(x => x.PaymentMethodId, q.PaymentMethodId.Value));

        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }
}
