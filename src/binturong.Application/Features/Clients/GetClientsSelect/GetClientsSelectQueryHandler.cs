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

namespace Application.Features.Clients.GetClientsSelect;

internal sealed class GetClientsSelectQueryHandler
    : IQueryHandler<GetClientsSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Clients";
    private const string Entity = "Client";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetClientsSelectQueryHandler(
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
        GetClientsSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.TradeName)
            .ThenBy(x => x.ContactName)
            .ThenBy(x => x.Identification)
            .Limit(MaxSelectResults)
            .Project(x => new SelectOptionDto(x.ClientId.ToString(), BuildLabel(x), BuildCode(x)))
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "CLIENTS_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(docs);
    }

    private static FilterDefinition<ClientReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<ClientReadModel>.Filter;
        var filters = new List<FilterDefinition<ClientReadModel>>();

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
                    builder.Regex(x => x.PrimaryPhone, startsWithRegex),
                    builder.Regex(x => x.SecondaryPhone, startsWithRegex),
                    builder.Regex(x => x.Email, startsWithRegex),
                    builder.Regex(x => x.TradeName, containsRegex),
                    builder.Regex(x => x.ContactName, containsRegex),
                    builder.Regex(x => x.Industry, containsRegex),
                    builder.Regex(x => x.ClientType, containsRegex)
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

    private static string BuildLabel(ClientReadModel x)
    {
        var tradeName = x.TradeName?.Trim();
        var contactName = x.ContactName?.Trim();
        var identification = x.Identification?.Trim();

        if (!string.IsNullOrWhiteSpace(tradeName) && !string.IsNullOrWhiteSpace(identification))
        {
            return $"{tradeName} - {identification}";
        }

        if (!string.IsNullOrWhiteSpace(tradeName) && !string.IsNullOrWhiteSpace(contactName))
        {
            return $"{tradeName} - {contactName}";
        }

        if (!string.IsNullOrWhiteSpace(tradeName))
        {
            return tradeName;
        }

        if (!string.IsNullOrWhiteSpace(contactName) && !string.IsNullOrWhiteSpace(identification))
        {
            return $"{contactName} - {identification}";
        }

        if (!string.IsNullOrWhiteSpace(contactName))
        {
            return contactName;
        }

        if (!string.IsNullOrWhiteSpace(identification))
        {
            return identification;
        }

        return x.ClientId.ToString();
    }

    private static string? BuildCode(ClientReadModel x)
    {
        if (!string.IsNullOrWhiteSpace(x.Identification))
            return x.Identification;

        if (!string.IsNullOrWhiteSpace(x.Email))
            return x.Email;

        if (!string.IsNullOrWhiteSpace(x.PrimaryPhone))
            return x.PrimaryPhone;

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
