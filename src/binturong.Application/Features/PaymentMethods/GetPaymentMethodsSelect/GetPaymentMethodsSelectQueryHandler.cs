using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.PaymentMethods.GetPaymentMethodsSelect;

internal sealed class GetPaymentMethodsSelectQueryHandler
    : IQueryHandler<GetPaymentMethodsSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "PaymentMethods";
    private const string Entity = "PaymentMethod";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPaymentMethodsSelectQueryHandler(
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

    public async Task<Result<IReadOnlyList<SelectOptionDto>>> Handle(
        GetPaymentMethodsSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PaymentMethodReadModel>(MongoCollections.PaymentMethods);

        var filter = BuildFilter(query.Search, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Code)
            .Project(x => new SelectOptionDto(x.PaymentMethodId.ToString(), $"{x.Code}", x.Code))
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "PAYMENT_METHODS_SELECT_READ",
                string.Empty,
                $"search={query.Search ?? ""}; onlyActive={query.OnlyActive}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(docs);
    }

    private static FilterDefinition<PaymentMethodReadModel> BuildFilter(
        string? search,
        bool onlyActive
    )
    {
        var builder = Builders<PaymentMethodReadModel>.Filter;
        var filters = new List<FilterDefinition<PaymentMethodReadModel>>();

        if (onlyActive)
            filters.Add(builder.Eq(x => x.IsActive, true));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var re = new BsonRegularExpression(s, "i");

            filters.Add(builder.Or(builder.Regex(x => x.Code, re), builder.Regex(x => x.Code, re)));
        }

        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }
}
