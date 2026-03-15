using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Products.GetProductsSelect;

internal sealed class GetProductsSelectQueryHandler
    : IQueryHandler<GetProductsSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Products";
    private const string Entity = "Product";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetProductsSelectQueryHandler(
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
        GetProductsSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ProductReadModel>(MongoCollections.Products);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .ThenBy(x => x.SKU)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.ProductId.ToString(),
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
                "PRODUCTS_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={result.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<ProductReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<ProductReadModel>.Filter;
        var filters = new List<FilterDefinition<ProductReadModel>>();

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
                    builder.Regex(x => x.SKU, startsWithRegex),
                    builder.Regex(x => x.Barcode, startsWithRegex),
                    builder.Regex(x => x.Name, containsRegex),
                    builder.Regex(x => x.Description, containsRegex),
                    builder.Regex(x => x.CategoryName, containsRegex)
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

    private static string BuildLabel(ProductReadModel x)
    {
        var sku = x.SKU?.Trim();
        var name = x.Name?.Trim();

        if (!string.IsNullOrWhiteSpace(sku) && !string.IsNullOrWhiteSpace(name))
            return $"{sku} - {name}";

        if (!string.IsNullOrWhiteSpace(name))
            return name;

        if (!string.IsNullOrWhiteSpace(sku))
            return sku;

        return x.ProductId.ToString();
    }

    private static string? BuildCode(ProductReadModel x)
    {
        if (!string.IsNullOrWhiteSpace(x.SKU))
            return x.SKU;

        if (!string.IsNullOrWhiteSpace(x.Barcode))
            return x.Barcode;

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
