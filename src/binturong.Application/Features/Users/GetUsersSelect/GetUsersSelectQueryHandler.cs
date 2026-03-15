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

namespace Application.Features.Users.GetUsersSelect;

internal sealed class GetUsersSelectQueryHandler
    : IQueryHandler<GetUsersSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Users";
    private const string Entity = "User";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetUsersSelectQueryHandler(
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
        GetUsersSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Username)
            .ThenBy(x => x.Email)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.UserId.ToString(),
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
                "USERS_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={result.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<UserReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<UserReadModel>.Filter;
        var filters = new List<FilterDefinition<UserReadModel>>();

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
                    builder.Regex(x => x.Username, startsWithRegex),
                    builder.Regex(x => x.Email, startsWithRegex),
                    builder.Regex(x => x.Username, containsRegex),
                    builder.Regex(x => x.Email, containsRegex)
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

    private static string BuildLabel(UserReadModel x)
    {
        var username = x.Username?.Trim();
        var email = x.Email?.Trim();

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(email))
            return $"{username} - {email}";

        if (!string.IsNullOrWhiteSpace(email))
            return email;

        if (!string.IsNullOrWhiteSpace(username))
            return username;

        return x.UserId.ToString();
    }

    private static string? BuildCode(UserReadModel x)
    {
        if (!string.IsNullOrWhiteSpace(x.Email))
            return x.Email;

        if (!string.IsNullOrWhiteSpace(x.Username))
            return x.Username;

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
