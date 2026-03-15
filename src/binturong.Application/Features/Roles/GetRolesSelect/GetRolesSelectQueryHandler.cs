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

        var filter = BuildFilter(query.Search, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .ThenBy(x => x.Name)
            .Project(x => new SelectOptionDto(
                x.RoleId.ToString(),
                !string.IsNullOrWhiteSpace(x.Name) ? $"{x.Name}" : x.Name,
                x.Name
            ))
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "ROLES_SELECT_READ",
                string.Empty,
                $"search={query.Search ?? ""}; onlyActive={query.OnlyActive}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(docs);
    }

    private static FilterDefinition<RoleReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<RoleReadModel>.Filter;
        var filters = new List<FilterDefinition<RoleReadModel>>();

        if (onlyActive)
            filters.Add(builder.Eq(x => x.IsActive, true));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var re = new BsonRegularExpression(s, "i");

            filters.Add(builder.Or(builder.Regex(x => x.Name, re), builder.Regex(x => x.Name, re)));
        }

        return filters.Count switch
        {
            0 => builder.Empty,
            1 => filters[0],
            _ => builder.And(filters),
        };
    }
}
