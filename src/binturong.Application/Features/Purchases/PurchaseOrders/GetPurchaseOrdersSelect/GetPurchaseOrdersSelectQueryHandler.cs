using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.Purchases;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.Orders.GetPurchaseOrdersSelect;

internal sealed class GetPurchaseOrdersSelectQueryHandler
    : IQueryHandler<GetPurchaseOrdersSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "PurchaseOrders";
    private const string Entity = "PurchaseOrder";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseOrdersSelectQueryHandler(
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
        GetPurchaseOrdersSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(MongoCollections.PurchaseOrders);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.OrderDate)
            .ThenBy(x => x.Code)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.PurchaseOrderId.ToString(),
                BuildLabel(x),
                BuildCode(x)
            ))
            .ToList();

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "PURCHASE_ORDERS_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; limit={MaxSelectResults}; returned={result.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<PurchaseOrderReadModel> BuildFilter(string? search)
    {
        var builder = Builders<PurchaseOrderReadModel>.Filter;

        if (string.IsNullOrWhiteSpace(search))
        {
            return builder.Empty;
        }

        var escaped = Regex.Escape(search);
        var startsWithRegex = new BsonRegularExpression($"^{escaped}", "i");
        var containsRegex = new BsonRegularExpression(escaped, "i");

        return builder.Or(
            builder.Regex(x => x.Code, startsWithRegex),
            builder.Regex(x => x.Status, containsRegex),
            builder.Regex(x => x.SupplierName, containsRegex),
            builder.Regex(x => x.BranchName, containsRegex),
            builder.Regex(x => x.Currency, containsRegex)
        );
    }

    private static string BuildLabel(PurchaseOrderReadModel x)
    {
        var code = x.Code?.Trim();
        var supplierName = x.SupplierName?.Trim();
        var branchName = x.BranchName?.Trim();

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(supplierName))
        {
            return $"{code} - {supplierName}";
        }

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(branchName))
        {
            return $"{code} - {branchName}";
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        return x.PurchaseOrderId.ToString();
    }

    private static string? BuildCode(PurchaseOrderReadModel x)
    {
        if (!string.IsNullOrWhiteSpace(x.Code))
            return x.Code;

        return x.Currency;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
