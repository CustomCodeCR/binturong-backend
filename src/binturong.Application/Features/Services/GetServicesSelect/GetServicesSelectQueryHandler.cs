using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Services.GetServicesSelect;

internal sealed class GetServicesSelectQueryHandler
    : IQueryHandler<GetServicesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Services";
    private const string Entity = "Service";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetServicesSelectQueryHandler(
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
        GetServicesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ServiceReadModel>(MongoCollections.Services);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch);

        var docs = await col.Find(filter)
            .SortBy(x => x.CategoryName)
            .ThenBy(x => x.Name)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.ServiceId.ToString(),
                BuildLabel(x),
                x.Code
            ))
            .ToList();

        await _bus.AuditAsync(
            _currentUser.UserId,
            Module,
            Entity,
            null,
            "SERVICE_SELECT_READ",
            string.Empty,
            $"search={normalizedSearch ?? ""}; limit={MaxSelectResults}; returned={result.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<ServiceReadModel> BuildFilter(string? search)
    {
        var builder = Builders<ServiceReadModel>.Filter;

        var baseFilter = builder.And(
            builder.Eq(x => x.IsActive, true),
            builder.Eq(x => x.AvailabilityStatus, "Active")
        );

        if (string.IsNullOrWhiteSpace(search))
            return baseFilter;

        var escaped = Regex.Escape(search);
        var startsWithRegex = new BsonRegularExpression($"^{escaped}", "i");
        var containsRegex = new BsonRegularExpression(escaped, "i");

        var searchFilter = builder.Or(
            builder.Regex(x => x.Code, startsWithRegex),
            builder.Regex(x => x.Name, containsRegex),
            builder.Regex(x => x.CategoryName, containsRegex),
            builder.Regex(x => x.Description, containsRegex)
        );

        return builder.And(baseFilter, searchFilter);
    }

    private static string BuildLabel(ServiceReadModel x)
    {
        var code = x.Code?.Trim();
        var name = x.Name?.Trim();
        var category = x.CategoryName?.Trim();

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
        {
            if (!string.IsNullOrWhiteSpace(category))
                return $"{code} - {name} ({category})";

            return $"{code} - {name}";
        }

        if (!string.IsNullOrWhiteSpace(name))
            return name;

        if (!string.IsNullOrWhiteSpace(code))
            return code;

        return x.ServiceId.ToString();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
