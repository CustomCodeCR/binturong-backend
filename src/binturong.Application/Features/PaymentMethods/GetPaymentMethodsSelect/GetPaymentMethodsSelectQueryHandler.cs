using System.Text.RegularExpressions;
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
    private const int MaxSelectResults = 100;

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

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Code)
            .ThenBy(x => x.Description)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.PaymentMethodId.ToString(),
                BuildLabel(x),
                x.Code
            ))
            .ToList();

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "PAYMENT_METHODS_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={result.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<PaymentMethodReadModel> BuildFilter(
        string? search,
        bool onlyActive
    )
    {
        var builder = Builders<PaymentMethodReadModel>.Filter;
        var filters = new List<FilterDefinition<PaymentMethodReadModel>>();

        if (onlyActive)
        {
            filters.Add(builder.Eq(x => x.IsActive, true));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = Regex.Escape(search);
            var startsWithRegex = new BsonRegularExpression($"^{escaped}", "i");
            var containsRegex = new BsonRegularExpression(escaped, "i");

            filters.Add(
                builder.Or(
                    builder.Regex(x => x.Code, startsWithRegex),
                    builder.Regex(x => x.Description, containsRegex)
                )
            );
        }

        return filters.Count switch
        {
            0 => builder.Empty,
            1 => filters[0],
            _ => builder.And(filters),
        };
    }

    private static string BuildLabel(PaymentMethodReadModel x)
    {
        var code = x.Code?.Trim();
        var description = x.Description?.Trim();

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(description))
            return $"{code} - {description}";

        if (!string.IsNullOrWhiteSpace(code))
            return code;

        if (!string.IsNullOrWhiteSpace(description))
            return description;

        return x.PaymentMethodId.ToString();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
