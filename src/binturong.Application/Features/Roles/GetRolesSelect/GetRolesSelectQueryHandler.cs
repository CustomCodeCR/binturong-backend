using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Roles.GetRolesSelect;

internal sealed class GetRolesSelectQueryHandler
    : IQueryHandler<GetRolesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Roles";
    private const string Entity = "Role";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetRolesSelectQueryHandler(
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
        GetRolesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .ThenBy(x => x.Description)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.RoleId.ToString(),
                BuildLabel(x),
                x.Name
            ))
            .ToList();

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "ROLES_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={result.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<RoleReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<RoleReadModel>.Filter;
        var filters = new List<FilterDefinition<RoleReadModel>>();

        if (onlyActive)
        {
            filters.Add(builder.Eq(x => x.IsActive, true));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = Regex.Escape(search);
            var containsRegex = new BsonRegularExpression(escaped, "i");

            filters.Add(
                builder.Or(
                    builder.Regex(x => x.Name, containsRegex),
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

    private static string BuildLabel(RoleReadModel x)
    {
        var name = x.Name?.Trim();

        if (!string.IsNullOrWhiteSpace(name))
            return name;

        return x.RoleId.ToString();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
