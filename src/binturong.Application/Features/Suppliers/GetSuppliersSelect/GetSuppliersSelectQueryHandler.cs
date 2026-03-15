using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Suppliers.GetSuppliersSelect;

internal sealed class GetSuppliersSelectQueryHandler
    : IQueryHandler<GetSuppliersSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Suppliers";
    private const string Entity = "Supplier";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSuppliersSelectQueryHandler(
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
        GetSuppliersSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.TradeName)
            .ThenBy(x => x.LegalName)
            .ThenBy(x => x.Identification)
            .Limit(MaxSelectResults)
            .Project(x => new SelectOptionDto(x.SupplierId.ToString(), BuildLabel(x), BuildCode(x)))
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "SUPPLIERS_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(docs);
    }

    private static FilterDefinition<SupplierReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<SupplierReadModel>.Filter;
        var filters = new List<FilterDefinition<SupplierReadModel>>();

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
                    builder.Regex(x => x.Identification, startsWithRegex),
                    builder.Regex(x => x.Phone, startsWithRegex),
                    builder.Regex(x => x.Email, startsWithRegex),
                    builder.Regex(x => x.TradeName, containsRegex),
                    builder.Regex(x => x.LegalName, containsRegex),
                    builder.Regex(x => x.PaymentTerms, containsRegex),
                    builder.Regex(x => x.MainCurrency, containsRegex)
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

    private static string BuildLabel(SupplierReadModel x)
    {
        var tradeName = x.TradeName?.Trim();
        var legalName = x.LegalName?.Trim();
        var identification = x.Identification?.Trim();

        if (!string.IsNullOrWhiteSpace(tradeName) && !string.IsNullOrWhiteSpace(identification))
        {
            return $"{tradeName} - {identification}";
        }

        if (!string.IsNullOrWhiteSpace(legalName) && !string.IsNullOrWhiteSpace(identification))
        {
            return $"{legalName} - {identification}";
        }

        if (
            !string.IsNullOrWhiteSpace(tradeName)
            && !string.IsNullOrWhiteSpace(legalName)
            && !tradeName.Equals(legalName, StringComparison.OrdinalIgnoreCase)
        )
        {
            return $"{tradeName} - {legalName}";
        }

        if (!string.IsNullOrWhiteSpace(tradeName))
        {
            return tradeName;
        }

        if (!string.IsNullOrWhiteSpace(legalName))
        {
            return legalName;
        }

        if (!string.IsNullOrWhiteSpace(identification))
        {
            return identification;
        }

        return x.SupplierId.ToString();
    }

    private static string? BuildCode(SupplierReadModel x)
    {
        if (!string.IsNullOrWhiteSpace(x.Identification))
            return x.Identification;

        if (!string.IsNullOrWhiteSpace(x.Email))
            return x.Email;

        if (!string.IsNullOrWhiteSpace(x.Phone))
            return x.Phone;

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
